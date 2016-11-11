// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Internals.Scorables;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Associate a LUIS intent with a dialog method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [Serializable]
    public class LuisIntentAttribute : AttributeString
    {
        /// <summary>
        /// The LUIS intent name.
        /// </summary>
        public readonly string IntentName;

        /// <summary>
        /// Construct the association between the LUIS intent and a dialog method.
        /// </summary>
        /// <param name="intentName">The LUIS intent name.</param>
        public LuisIntentAttribute(string intentName)
        {
            SetField.NotNull(out this.IntentName, nameof(intentName), intentName);
        }

        protected override string Text
        {
            get
            {
                return this.IntentName;
            }
        }
    }

    /// <summary>
    /// The handler for a LUIS intent.
    /// </summary>
    /// <param name="context">The dialog context.</param>
    /// <param name="luisResult">The LUIS result.</param>
    /// <returns>A task representing the completion of the intent processing.</returns>
    public delegate Task IntentHandler(IDialogContext context, LuisResult luisResult);

    /// <summary>
    /// The handler for a LUIS intent.
    /// </summary>
    /// <param name="context">The dialog context.</param>
    /// <param name="message">The dialog message.</param>
    /// <param name="luisResult">The LUIS result.</param>
    /// <returns>A task representing the completion of the intent processing.</returns>
    public delegate Task IntentActivityHandler(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult);

    /// <summary>
    /// An exception for invalid intent handlers.
    /// </summary>
    [Serializable]
    public sealed class InvalidIntentHandlerException : InvalidOperationException
    {
        public readonly MethodInfo Method;

        public InvalidIntentHandlerException(string message, MethodInfo method)
            : base(message)
        {
            SetField.NotNull(out this.Method, nameof(method), method);
        }

        private InvalidIntentHandlerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// Matches a LuisResult object with the best scored IntentRecommendation of the LuisResult and corresponding Luis service
    /// </summary>
    public class LuisServiceResult
    {
        public LuisServiceResult(LuisResult result, IntentRecommendation intent, ILuisService service)
        {
            this.Result = result;
            this.BestIntent = intent;
            this.LuisService = service;
        }

        public LuisResult Result { get; }

        public IntentRecommendation BestIntent { get; }

        public ILuisService LuisService { get; }
    }

    /// <summary>
    /// A dialog specialized to handle intents and entities from LUIS.
    /// </summary>
    [Serializable]
    public class LuisDialog<R> : IDialog<R>
    {
        protected readonly IReadOnlyList<ILuisService> services;

        /// <summary>   Mapping from intent string to the appropriate handler. </summary>
        [NonSerialized]
        protected Dictionary<string, IntentActivityHandler> handlerByIntent;

        public ILuisService[] MakeServicesFromAttributes()
        {
            var type = this.GetType();
            var luisModels = type.GetCustomAttributes<LuisModelAttribute>(inherit: true);
            return luisModels.Select(m => new LuisService(m)).Cast<ILuisService>().ToArray();
        }

        /// <summary>
        /// Construct the LUIS dialog.
        /// </summary>
        /// <param name="services">The LUIS service.</param>
        public LuisDialog(params ILuisService[] services)
        {
            if (services.Length == 0)
            {
                services = MakeServicesFromAttributes();
            }

            SetField.NotNull(out this.services, nameof(services), services);
        }

        public virtual async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceived);
        }

        /// <summary>
        /// Calculates the best scored <see cref="IntentRecommendation" /> from a <see cref="LuisResult" />.
        /// </summary>
        /// <param name="result">A result of a LUIS service call.</param>
        /// <returns>The best scored <see cref="IntentRecommendation" />, or null if <paramref name="result" /> doesn't contain any intents.</returns>
        protected virtual IntentRecommendation BestIntentFrom(LuisResult result)
        {
            return result.TopScoringIntent ?? result.Intents?.MaxBy(i => i.Score ?? 0d);
        }

        /// <summary>
        /// Calculates the best scored <see cref="IntentRecommendation" /> across some <see cref="LuisResult" /> objects.
        /// </summary>
        /// <param name="results">Results of multiple LUIS services calls.</param>
        /// <returns>A <see cref="LuisServiceResult" /> with the best scored <see cref="IntentRecommendation" /> and related <see cref="LuisResult" />,
        /// or null if no one of <paramref name="results" /> contains any intents.</returns>
        protected virtual LuisServiceResult BestResultFrom(IEnumerable<LuisServiceResult> results)
        {
            return results.MaxBy(i => i.BestIntent.Score ?? 0d);
        }

        protected virtual async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;
            var messageText = await GetLuisQueryTextAsync(context, message);
            
            var tasks = this.services.Select(s => s.QueryAsync(messageText, context.CancellationToken)).ToArray();
            var results = await Task.WhenAll(tasks);

            var winners = from result in results.Select((value, index) => new {value, index} )
                          let resultWinner = BestIntentFrom(result.value)
                          where resultWinner != null
                          select new LuisServiceResult(result.value, resultWinner, this.services[result.index]);

            var winner = this.BestResultFrom(winners);

            if (winner?.Result.Dialog?.Status == DialogResponse.DialogStatus.Question)
            {
                var childDialog = await LuisActionDialog(winner);
                context.Call(childDialog, LuisActionDialogFinished);
            }
            else
            {
                await DispatchToIntentHandler(context, item, winner?.BestIntent, winner?.Result);
            }
        }

        protected virtual async Task DispatchToIntentHandler(IDialogContext context, 
                                                            IAwaitable<IMessageActivity> item,  
                                                            IntentRecommendation bestInent, 
                                                            LuisResult result)
        {
            if (this.handlerByIntent == null)
            {
                this.handlerByIntent = new Dictionary<string, IntentActivityHandler>(GetHandlersByIntent());
            }

            IntentActivityHandler handler = null;
            if (result == null || !this.handlerByIntent.TryGetValue(bestInent.Intent, out handler))
            {
                handler = this.handlerByIntent[string.Empty];
            }

            if (handler != null)
            {
                await handler(context, item, result);
            }
            else
            {
                var text = $"No default intent handler found.";
                throw new Exception(text);
            }
        }

        protected virtual Task<string> GetLuisQueryTextAsync(IDialogContext context, IMessageActivity message)
        {
            return Task.FromResult(message.Text);
        }

        protected virtual IDictionary<string, IntentActivityHandler> GetHandlersByIntent()
        {
            return LuisDialog.EnumerateHandlers(this).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        protected virtual async Task<IDialog<LuisResult>> LuisActionDialog(LuisServiceResult result)
        {
            return  new LuisActionDialog(result);
        }

        protected virtual async Task LuisActionDialogFinished(IDialogContext context, IAwaitable<LuisResult> item)
        {
            var result = await item;
            var messageActivity = (IMessageActivity) context.Activity;
            await DispatchToIntentHandler(context, messageActivity.GetAwaitable(), BestIntentFrom(result), result);
        }
    }

    [Serializable]
    internal class LuisActionDialog : IDialog<LuisResult>
    {
        private readonly ILuisService luisService;
        private string contextId;
        private string prompt;

        public LuisActionDialog(LuisServiceResult result)
        {
            SetField.NotNull(out this.luisService, nameof(luisService), result.LuisService);
            SetField.NotNull(out this.contextId, nameof(contextId), result.Result.Dialog.ContextId);
            this.prompt = result.Result.Dialog.Prompt;
        }


        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(this.prompt);
            context.Wait(MessageReceived);
        }

        protected async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;
            var result = await luisService.QueryAsync(message.Text, this.contextId, context.CancellationToken);
            if (result.Dialog.Status != DialogResponse.DialogStatus.Finished)
            {
                this.contextId = result.Dialog.ContextId;
                this.prompt = result.Dialog.Prompt;
                await context.PostAsync(this.prompt);
                context.Wait(MessageReceived);
            }
            else
            {
                context.Done(result);
            }
        }
    }

    internal static class LuisDialog
    {
        /// <summary>
        /// Enumerate the handlers based on the attributes on the dialog instance.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <returns>An enumeration of handlers.</returns>
        public static IEnumerable<KeyValuePair<string, IntentActivityHandler>> EnumerateHandlers(object dialog)
        {
            var type = dialog.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var method in methods)
            {
                var intents = method.GetCustomAttributes<LuisIntentAttribute>(inherit: true).ToArray();
                IntentActivityHandler intentHandler = null;

                try
                {
                    intentHandler = (IntentActivityHandler)Delegate.CreateDelegate(typeof(IntentActivityHandler), dialog, method, throwOnBindFailure: false);
                }
                catch (ArgumentException)
                {
                    // "Cannot bind to the target method because its signature or security transparency is not compatible with that of the delegate type."
                    // https://github.com/Microsoft/BotBuilder/issues/634
                    // https://github.com/Microsoft/BotBuilder/issues/435
                }

                // fall back for compatibility
                if (intentHandler == null)
                {
                    try
                    {
                        var handler = (IntentHandler)Delegate.CreateDelegate(typeof(IntentHandler), dialog, method, throwOnBindFailure: false);

                        if (handler != null)
                        {
                            // thunk from new to old delegate type
                            intentHandler = (context, message, result) => handler(context, result);
                        }
                    }
                    catch (ArgumentException)
                    {
                        // "Cannot bind to the target method because its signature or security transparency is not compatible with that of the delegate type."
                        // https://github.com/Microsoft/BotBuilder/issues/634
                        // https://github.com/Microsoft/BotBuilder/issues/435
                    }
                }

                if (intentHandler != null)
                {
                    var intentNames = intents.Select(i => i.IntentName).DefaultIfEmpty(method.Name);

                    foreach (var intentName in intentNames)
                    {
                        var key = string.IsNullOrWhiteSpace(intentName) ? string.Empty : intentName;
                        yield return new KeyValuePair<string, IntentActivityHandler>(intentName, intentHandler);
                    }
                }
                else
                {
                    if (intents.Length > 0)
                    {
                        throw new InvalidIntentHandlerException(string.Join(";", intents.Select(i => i.IntentName)), method);
                    }
                }
            }
        }
    }
}