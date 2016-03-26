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
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class LuisModel : Attribute
    {
        public readonly string luisModelUrl;

        public LuisModel(string luisModelUrl)
        {
            Field.SetNotNull(out this.luisModelUrl, nameof(luisModelUrl), luisModelUrl);
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class LuisIntent : Attribute
    {
        public readonly string intentName;

        public LuisIntent(string intentName)
        {
            Field.SetNotNull(out this.intentName, nameof(intentName), intentName);
        }
    }

    public class LuisResult
    {
        public Models.IntentRecommendation[] Intents { get; set; }

        public Models.EntityRecommendation[] Entities { get; set; }
    }

    public delegate Task IntentHandler(IDialogContext context, LuisResult luisResult);

    [Serializable]
    public class LuisDialog : IDialog<object>, ISerializable
    {
        public readonly string subscriptionKey;
        public readonly string modelID;
        public readonly string luisUrl;

        protected readonly Dictionary<string, IntentHandler> handlerByIntent = new Dictionary<string, IntentHandler>();
        protected const string DefaultIntentHandler = "87DBD4FD7736";

        public LuisDialog()
        {
            var luisModel = ((LuisModel)this.GetType().GetCustomAttributes(typeof(LuisModel), true).FirstOrDefault())?.luisModelUrl;

            if (!string.IsNullOrEmpty(luisModel))
            {
                this.luisUrl = luisModel;
            }
            else
            {
                throw new Exception("Luis model attribute is not set for the class");
            }

            this.AddAttributeBasedHandlers();
        }

        public LuisDialog(string subscriptionKey, string modelID)
        {
            Field.SetNotNull(out this.subscriptionKey, nameof(subscriptionKey), subscriptionKey);
            Field.SetNotNull(out this.modelID, nameof(modelID), modelID);
            this.luisUrl = string.Format("https://api.projectoxford.ai/luis/v1/application?id={0}&subscription-key={1}&q=", this.modelID, this.subscriptionKey);
        }

        protected LuisDialog(SerializationInfo info, StreamingContext context)
        {
            Field.SetNotNullFrom(out this.luisUrl, nameof(this.luisUrl), info);
            Field.SetFrom(out this.subscriptionKey, nameof(this.subscriptionKey), info);
            Field.SetFrom(out this.modelID, nameof(this.modelID), info);

            this.AddAttributeBasedHandlers();
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.subscriptionKey), this.subscriptionKey);
            info.AddValue(nameof(this.luisUrl), this.luisUrl);
            info.AddValue(nameof(this.modelID), this.modelID);
        }

        public virtual async Task StartAsync(IDialogContext context, IAwaitable<object> argument)
        {
            context.Wait(MessageReceived);
        }

        protected virtual async Task<LuisResult> GetLuisResult(string luisUrl, string text)
        {
            var url = luisUrl + Uri.EscapeDataString(text);
            string json;
            using (HttpClient client = new HttpClient())
            {
                json = await client.GetStringAsync(url);
            }

            Debug.WriteLine(json);
            var response = JsonConvert.DeserializeObject<LuisResult>(json);
            return response;
        }

        protected async Task MessageReceived(IDialogContext context, IAwaitable<Message> item)
        {
            var message = await item;
            var luisRes = await GetLuisResult(this.luisUrl, message.Text);
            var intent = luisRes.Intents.FirstOrDefault(i => i.Score == luisRes.Intents.Select(t => t.Score).Max());
            IntentHandler handler;
            if (intent == null || !this.handlerByIntent.TryGetValue(intent.Intent, out handler))
            {
                handler = this.handlerByIntent[DefaultIntentHandler];
            }

            if (handler != null)
            {
                await handler(context, luisRes);
            }
            else
            {
                var text = $"LuisModel[{this.modelID}] no default intent handler.";
                throw new Exception(text);
            }
        }

        private void AddAttributeBasedHandlers()
        {
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
                    var key = string.IsNullOrEmpty(intent) ? DefaultIntentHandler : intent;
                    this.handlerByIntent.Add(key, intentHandler);
                }
            }
        }
    }
}
