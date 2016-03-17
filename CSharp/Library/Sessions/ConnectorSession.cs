using Microsoft.Bot.Connector;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Builder
{
    public class ConnectorSession : Session
    {
        public static readonly StoreType storeType = StoreType.MessageStore;

        private HttpRequestMessage request;

        
        public ConnectorSession(Message msg, ISessionData sessionData, IDialogStack stack, IDialog defaultDialog, HttpRequestMessage request)
            : base(msg, sessionData, stack, defaultDialog)
        {
            this.request = request;
        }

        public enum StoreType
        {
            CookieStore,
            MessageStore
        }
       

        // TODO: remove this static method and static GetStore
        public static async Task<HttpResponseMessage> MessageReceivedAsync(HttpRequestMessage request, Message msg, IDialogCollection dialogs, IDialog defaultDialog)
        {
            var store = GetStore(request, msg);
            var reply = await Session.DispatchAsync(
                store,
                msg,
                dialogs,
                (data, stack) => new ConnectorSession(msg, data, stack, defaultDialog, request),
                (session, response) => session.ComposeReply(store, response));

            return reply;
        }

        protected HttpResponseMessage ComposeReply(ISessionStore store, Task<Connector.Message> task)
        {
            HttpResponseMessage response;
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    var fromBot = task.Result;
                    if (store is MessageStore)
                    {
                        fromBot.BotUserData = Message.BotUserData;
                        fromBot.BotConversationData = Message.BotConversationData;
                        fromBot.BotPerUserInConversationData = Message.BotPerUserInConversationData;
                    }
                    response = this.request.CreateResponse(HttpStatusCode.OK, fromBot);
                    break;
                case TaskStatus.Faulted:
                    var error = task.Exception.InnerException as HttpException;
                    var status = error != null ? (HttpStatusCode)error.GetHttpCode() : HttpStatusCode.InternalServerError;
                    response = this.request.CreateResponse(status);
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (store is CookieStore)
            {
                ((CookieStore)store).AddSavedStateToCookies(ref response);
            }

            return response; 
        }


        private static ISessionStore GetStore(HttpRequestMessage request, Message msg)
        {
            ISessionStore store;
            switch (storeType)
            {
                default:
                case StoreType.CookieStore:
                    store = new CookieStore(request);
                    break;
                case StoreType.MessageStore:
                    store = new MessageStore(msg);
                    break;
            }
            return store;
        }

    }
}
