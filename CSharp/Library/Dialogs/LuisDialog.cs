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
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Associate a LUIS intent with a dialog method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class LuisIntentAttribute : Attribute
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
    }

    /// <summary>
    /// The handler for a LUIS intent.
    /// </summary>
    /// <param name="context">The dialog context.</param>
    /// <param name="luisResult">The LUIS result.</param>
    /// <returns>A task representing the completion of the intent processing.</returns>
    public delegate Task IntentHandler(IDialogContext context, LuisResult luisResult);

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
    /// Matches a LuisResult object with the best scored IntentRecommendation of the LuisResult.
    /// </summary>
    public class LuisServiceResult
    {
        public LuisServiceResult(LuisResult result, IntentRecommendation intent)
        {
            this.Result = result;
            this.BestIntent = intent;
        }

        public LuisResult Result { get; }

        public IntentRecommendation BestIntent { get; }
    }

    /// <summary>
    /// A dialog specialized to handle intents and entities from LUIS.
    /// </summary>
    [Serializable]
    public class LuisDialog<R> : IDialog<R>
    {
        private readonly IReadOnlyList<ILuisService> services;

        /// <summary>   Mapping from intent string to the appropriate handler. </summary>
        [NonSerialized]
        protected Dictionary<string, IntentHandler> handlerByIntent;

        /// <summary>
        /// Construct the LUIS dialog.
        /// </summary>
        /// <param name="services">The LUIS service.</param>
        public LuisDialog(params ILuisService[] services)
        {
            var type = this.GetType();
            var luisModels = type.GetCustomAttributes<LuisModelAttribute>(inherit: true);
            services = services.Concat(luisModels.Select(m => new LuisService(m))).ToArray();
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
            return result.Intents.MaxBy(i => i.Score ?? 0d);
        }

        /// <summary>
        /// Calculates the best scored <see cref="IntentRecommendation" /> across some <see cref="LuisResult" /> objects.
        /// </summary>
        /// <param name="results">Results of multiple LUIS services calls.</param>
        /// <returns>A <see cref="LuisServiceResult" /> with the best scored <see cref="IntentRecommendation" /> and related <see cref="LuisResult" />,
        /// or null if no one of <paramref name="results" /> contains any intents.</returns>
        protected virtual LuisServiceResult BestResultFrom(IEnumerable<LuisResult> results)
        {
            var winners = from result in results
                          let resultWinner = BestIntentFrom(result)
                          where resultWinner != null
                          select new LuisServiceResult(result, resultWinner);
            return winners.MaxBy(i => i.BestIntent.Score ?? 0d);
        }

        protected virtual async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            if (this.handlerByIntent == null)
            {
                this.handlerByIntent = new Dictionary<string, IntentHandler>(GetHandlersByIntent());
            }

            var message = await item;
            var messageText = await GetLuisQueryTextAsync(context, message);
            var tasks = this.services.Select(s => s.QueryAsync(messageText)).ToArray();
            var winner = this.BestResultFrom(await Task.WhenAll(tasks));

            IntentHandler handler = null;
            if (winner == null || !this.handlerByIntent.TryGetValue(winner.BestIntent.Intent, out handler))
            {
                handler = this.handlerByIntent[string.Empty];
            }

            if (handler != null)
            {
                await handler(context, winner?.Result);
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

        protected virtual IDictionary<string, IntentHandler> GetHandlersByIntent()
        {
            return LuisDialog.EnumerateHandlers(this).ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }

    internal static class LuisDialog
    {
        /// <summary>
        /// Enumerate the handlers based on the attributes on the dialog instance.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <returns>An enumeration of handlers.</returns>
        public static IEnumerable<KeyValuePair<string, IntentHandler>> EnumerateHandlers(object dialog)
        {
            var type = dialog.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var method in methods)
            {
                var intents = method.GetCustomAttributes<LuisIntentAttribute>(inherit: true).ToArray();
                var intentHandler = (IntentHandler)Delegate.CreateDelegate(typeof(IntentHandler), dialog, method, throwOnBindFailure: false);
                if (intentHandler != null)
                {
                    var intentNames = intents.Select(i => i.IntentName).DefaultIfEmpty(method.Name);

                    foreach (var intentName in intentNames)
                    {
                        var key = string.IsNullOrWhiteSpace(intentName) ? string.Empty : intentName;
                        yield return new KeyValuePair<string, IntentHandler>(intentName, intentHandler);
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