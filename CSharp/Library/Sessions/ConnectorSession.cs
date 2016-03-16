using Microsoft.Bot.Connector;
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

        protected HttpResponseMessage ComposeReply(ISessionStore store, DialogResponse reply)
        {
            if (store is MessageStore)
            {
                reply.Reply.BotUserData = Message.BotUserData;
                reply.Reply.BotConversationData = Message.BotConversationData;
                reply.Reply.BotPerUserInConversationData = Message.BotPerUserInConversationData;
            }

            var status = reply.Error != null ? (HttpStatusCode)(reply.Error as HttpException)?.ErrorCode : HttpStatusCode.OK;

            var response = this.request.CreateResponse(status, reply.Reply);

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
