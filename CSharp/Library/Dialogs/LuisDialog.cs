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
using System.Threading.Tasks;

using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class LuisIntent : Attribute
    {
        public readonly string intentName;

        public LuisIntent(string intentName)
        {
            Field.SetNotNull(out this.intentName, nameof(intentName), intentName);
        }
    }


    public delegate Task IntentHandler(IDialogContext context, LuisResult luisResult);

    [Serializable]
    public class LuisDialog : IDialog
    {
        private readonly ILuisService service;

        [NonSerialized]
        protected Dictionary<string, IntentHandler> handlerByIntent;

        public LuisDialog()
        {
            var type = this.GetType();
            var luisModel = type.GetCustomAttribute<LuisModel>(inherit: true);
            if (luisModel == null)
            {
                throw new Exception("Luis model attribute is not set for the class");
            }

            this.service = new LuisService(luisModel);
        }

        public LuisDialog(ILuisService service)
        {
            Field.SetNotNull(out this.service, nameof(service), service);
        }

        public virtual async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceived);
        }

        protected async Task MessageReceived(IDialogContext context, IAwaitable<Message> item)
        {
            if (this.handlerByIntent == null)
            {
                this.AddAttributeBasedHandlers();
            }

            var message = await item;
            var luisRes = await this.service.QueryAsync(message.Text);

            var maximum = luisRes.Intents.Max(t => t.Score);
            var intent = luisRes.Intents.FirstOrDefault(i => i.Score == maximum);

            IntentHandler handler;
            if (intent == null || !this.handlerByIntent.TryGetValue(intent.Intent, out handler))
            {
                handler = this.handlerByIntent[string.Empty];
            }

            if (handler != null)
            {
                await handler(context, luisRes);
            }
            else
            {
                var text = $"No default intent handler found.";
                throw new Exception(text);
            }
        }

        private void AddAttributeBasedHandlers()
        {
            if (this.handlerByIntent != null)
            {
                throw new InvalidOperationException();
            }

            this.handlerByIntent = new Dictionary<string, IntentHandler>();

            var methods = from m in this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                          let attr = m.GetCustomAttributes(typeof(LuisIntent), true)
                          where attr.Length > 0
                          select new { method = m, attributes = attr.Select(s => (LuisIntent)s).ToList() };

            var intentHandlers = from m in methods
                                 select new { method = m.method, intents = m.attributes.Select(i => i.intentName) };

            foreach (var handler in intentHandlers)
            {
                // TODO: use handler.method.CreateDelegate?
                //var intentHandler = (IntentHandler) handler.method.CreateDelegate(typeof(IntentHandler));

                // assign to local variable to capture less in closure below
                var method = handler.method;
                var intentHandler = new IntentHandler(async (context, result) =>
                {
                    var task = (Task)method.Invoke(this, new object[] { context, result });
                    await task;
                });

                foreach (var intent in handler.intents)
                {
                    var key = string.IsNullOrWhiteSpace(intent) ? string.Empty : intent;
                    this.handlerByIntent.Add(key, intentHandler);
                }
            }
        }
    }
}
