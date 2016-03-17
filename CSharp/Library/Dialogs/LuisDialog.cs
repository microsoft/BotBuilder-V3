using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public delegate Task<Connector.Message> IntentHandler(ISession session, LuisResult luisResult);
    public delegate Task<Connector.Message> ResumeHandler<T>(ISession session, Task<T> taskResult);

    [AttributeUsage(AttributeTargets.Class, AllowMultiple =false)]
    public class LuisModel: Attribute
    {
        public readonly string luisModelUrl;

        public LuisModel(string luisModelUrl)
        {
            this.luisModelUrl = luisModelUrl; 
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class LuisIntent: Attribute
    {
        public readonly string intentName;

        public readonly bool resumeHandler; 
        
        public LuisIntent(string intentName, bool resumeHandler = false)
        {
            this.intentName = intentName;
            this.resumeHandler = resumeHandler; 
        }
    }

    public class LuisResult
    {
        public Models.IntentRecommendation[] Intents { get; set; }

        public Models.EntityRecommendation[] Entities { get; set; }
    }


    public class LuisDialog<TArgs, TResult> : Dialog<TArgs, TResult>
    {
        public readonly string subscriptionKey;
        public readonly string modelID;
        public readonly string luisUrl; 

        public class LuisIntentHandler
        {
            public string Intent { set; get; }
            public IntentHandler IntentHandler { set; get; }
            public ResumeHandler<TResult> ResumeHandler { set; get; }
        }

        protected Dictionary<string, LuisIntentHandler> _luisIntentHandler;
        protected const string DefaultIntentHandler = "87DBD4FD7736";
        private const string ActiveIntentField = "F844979349BD_INTENT";
        
        public LuisDialog()
            : base(ClassNameProvider)
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
            this._luisIntentHandler = new Dictionary<string, LuisIntentHandler>();
            this.AddAttributeBasedHandlers(); 
        }

        public LuisDialog(string dialogID, string subscriptionKey, string modelID)
            : base(dialogID)
        {
            this.subscriptionKey = subscriptionKey;
            this.modelID = modelID;
            this.luisUrl = String.Format("https://api.projectoxford.ai/luis/v1/application?id={0}&subscription-key={1}&q=", this.modelID, this.subscriptionKey);
            this._luisIntentHandler = new Dictionary<string, LuisIntentHandler>();
        }

        private void AddAttributeBasedHandlers()
        {
            var methods = from m in this.GetType().GetMethods()
                          let attr = m.GetCustomAttributes(typeof(LuisIntent), true)
                          where attr.Length > 0
                          select new { method = m, attributes = attr.Select(s => (LuisIntent)s).ToList() };

            var intentHandlers = from m in methods
                                 let resume = from attr in m.attributes where !attr.resumeHandler select attr
                                 where resume.Count() > 0
                                 select new { method = m.method, intents = resume.Select(i => i.intentName) };

            var resumeHandlers = from m in methods
                                 where !intentHandlers.Select(s => s.method).Contains(m.method)
                                 let resume = from attr in m.attributes where attr.resumeHandler select attr
                                 select new { method = m.method, intents = resume.Select(i => i.intentName) };
            
            foreach(var handler in intentHandlers)
            {
                // TODO: use handler.method.CreateDelegate?
                var intentHandler = new IntentHandler( async (session, taskResult) =>
                {
                    var task = (Task<Connector.Message>)handler.method.Invoke(this, new object[] { session, taskResult });
                    return await task; 
                });

                foreach(var intent in handler.intents)
                {
                    var resumeHandler = resumeHandlers.FirstOrDefault(r => r.intents.Contains(intent));
                    ResumeHandler<TResult> resume = null; 
                    if (resumeHandler != null && resumeHandler.method != null)
                    {
                        // TODO: use resumeHandler.method.CreateDelegate?
                        resume = new ResumeHandler<TResult>(async (session, taskResult) =>
                        {
                            var parameters = resumeHandler.method.GetParameters();
                            var taskType = parameters[1].ParameterType.GenericTypeArguments.Single();
                            var taskCasted = Tasks.Cast(taskResult, taskType);
                            var task = (Task<Connector.Message>)resumeHandler.method.Invoke(this, new object[] { session, taskCasted });
                            return await task;
                        });
                    }

                    var key = string.IsNullOrEmpty(intent) ? DefaultIntentHandler : intent;
                    this._luisIntentHandler[key] = new LuisIntentHandler
                    {
                        Intent = key,
                        ResumeHandler = resume,
                        IntentHandler = intentHandler
                    };
                }
                                            
            }
        }

        public override async Task<Connector.Message> ReplyReceivedAsync(ISession session)
        {
            var luisRes = await GetLuisResult(this.luisUrl, session.Message.Text);
            var intent = luisRes.Intents.FirstOrDefault(i => i.Score == luisRes.Intents.Select(t => t.Score).Max());
            LuisIntentHandler handler; 
            if (intent == null || ! this._luisIntentHandler.TryGetValue(intent.Intent, out handler))
            {
                handler = this._luisIntentHandler[DefaultIntentHandler];
            }

            if (handler != null)
            {
                session.Stack.SetLocal(ActiveIntentField, handler.Intent);
                return await handler.IntentHandler(session, luisRes);
            }
            else
            {
                var errMsg = string.Format("LuisModel[{0}] no default intent handler.", this.ID);
                throw new Exception(errMsg);
            }
        }

        public override async Task<Connector.Message> DialogResumedAsync(ISession session, Task<TResult> taskResult)
        {
            var activeIntent = session.Stack.GetLocal(ActiveIntentField) as string;
            var intent = this._luisIntentHandler[activeIntent];
            if (intent.ResumeHandler != null)
            {
                return await intent.ResumeHandler(session, taskResult);
            }
            else
            {
               return await base.DialogResumedAsync(session, taskResult);
            }
        }

        public LuisDialog<TArgs, TResult> On(string intent, IntentHandler intentHandler, ResumeHandler<TResult> resumeHandler = null)
        {
            this._luisIntentHandler[intent] = new LuisIntentHandler()
            {
                Intent = intent,
                IntentHandler = intentHandler,
                ResumeHandler = resumeHandler
            };

            return this; 
        }

        public LuisDialog<TArgs, TResult> OnDefault(IntentHandler intentHandler, ResumeHandler<TResult> resumeHandler = null)
        {
            return this.On(DefaultIntentHandler, intentHandler, resumeHandler);
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

        private static string ClassNameProvider<T,U>(Dialog<T, U> instance)
        {
            return instance.GetType().FullName;
        }
    }
}
