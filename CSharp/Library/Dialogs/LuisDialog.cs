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


    public class LuisDialog<TArgs, TResult> : Dialog<TArgs, TResult> where TResult : DialogResult
    {
        public readonly string subscriptionKey;
        public readonly string modelId;
        public readonly string luisUrl; 

        public class LuisIntentHandler
        {
            public string Intent { set; get; }
            public Func<ISession, LuisResult, Task<DialogResponse>> IntentHandler { set; get; }
            public DialogResumeHandler<DialogResult> ResumeHandler { set; get; }
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

        public LuisDialog(string dialogId, string subscriptionKey, string modelId)
            : base(dialogId)
        {
            this.subscriptionKey = subscriptionKey;
            this.modelId = modelId;
            this.luisUrl = String.Format("https://api.projectoxford.ai/luis/v1/application?id={0}&subscription-key={1}&q=", this.modelId, this.subscriptionKey);
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
                var intentHandler = new Func<ISession, LuisResult, Task<DialogResponse>>( async (session, res) =>
                {
                    var task = (Task<DialogResponse>)handler.method.Invoke(this, new object[] { session, res });
                    return await task; 
                });

                foreach(var intent in handler.intents)
                {
                    var resumeHandler = resumeHandlers.Where(r => r.intents.Contains(intent)).FirstOrDefault();
                    Func<ISession, DialogResult, Task<DialogResponse>> resume = null; 
                    if (resumeHandler != null && resumeHandler.method != null)
                    {
                        resume = new Func<ISession, DialogResult, Task<DialogResponse>>(async (session, res) =>
                        {
                            var task = (Task<DialogResponse>)resumeHandler.method.Invoke(this, new object[] { session, res });
                            return await task;
                        });
                    }

                    var key = string.IsNullOrEmpty(intent) ? DefaultIntentHandler : intent;
                    this._luisIntentHandler[key] = new LuisIntentHandler
                    {
                        Intent = key,
                        ResumeHandler = new DialogResumeHandler<DialogResult>() { HandlerAsync = resume },
                        IntentHandler = intentHandler
                    };
                }
                                            
            }
        }

        public override async Task<DialogResponse> ReplyReceivedAsync(ISession session)
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
                session.Stack.SetDialogState(ActiveIntentField, handler.Intent);
                return await handler.IntentHandler(session, luisRes);
            }
            else
            {
                var errMsg = string.Format("LuisModel[{0}] no default intent handler.", this.Id);
                Debug.Fail(errMsg);
                return await session.CreateDialogErrorResponse(errorMessage: errMsg);
            }
        }

        public override async Task<DialogResponse> DialogResumedAsync(ISession session, TResult result = null)
        {
            var activeIntent = session.Stack.GetDialogState(ActiveIntentField) as string;
            var intent = this._luisIntentHandler[activeIntent];
            if (intent.ResumeHandler != null)
            {
                return await intent.ResumeHandler.HandlerAsync(session, result);
            }
            else
            {
               return await base.DialogResumedAsync(session, result);
            }
        }

        public LuisDialog<TArgs, TResult> On(string intent, Func<ISession, LuisResult, Task<DialogResponse>> intentHandler, Func<ISession, DialogResult, Task<DialogResponse>> resumeHandler = null)
        {
            this._luisIntentHandler[intent] = new LuisIntentHandler()
            {
                Intent = intent,
                IntentHandler = intentHandler,
                ResumeHandler = new DialogResumeHandler<DialogResult>() { HandlerAsync = resumeHandler }
            };
            return this; 
        }

        public LuisDialog<TArgs, TResult> OnDefault(Func<ISession, LuisResult, Task<DialogResponse>> intentHandler, Func<ISession, DialogResult, Task<DialogResponse>> resumeHandler = null)
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

        private static string ClassNameProvider<T,U>(Dialog<T, U> instance) where U : DialogResult
        {
            return instance.GetType().FullName;
        }
    }
}
