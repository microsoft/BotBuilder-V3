using Microsoft.Bot.Connector;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Builder
{
    public class ConsoleSession : Session
    {
        public ConsoleSession(Message msg, ISessionData sessionData, IDialogStack stack, IDialog defaultDialog)
            : base(msg, sessionData, stack, defaultDialog)
        {
        }

        private static readonly ISessionStore SessionStore = new InMemoryStore();

        // TODO: remove this static method
        public static async Task<ConsoleReplyMessage> MessageReceivedAsync(Message msg, IDialogCollection dialogs, IDialog defaultDialog)
        {
            return await Session.DispatchAsync(
                SessionStore,
                msg,
                dialogs,
                (data, stack) => new ConsoleSession(msg, data, stack, defaultDialog),
                (session, reply) => Map(reply));
        }

        protected static ConsoleReplyMessage Map(DialogResponse msg)
        {
            return new ConsoleReplyMessage
            {
                Status = msg.Error != null ? (HttpStatusCode)(msg.Error as HttpException).ErrorCode : HttpStatusCode.OK,
                Msg = msg.Reply
            };
        }
        
        public class ConsoleReplyMessage
        {
            public HttpStatusCode Status { set; get; }
            public Message Msg { set; get; }
            public override string ToString()
            {
                return string.Format("status code: {0}, msg: {1}", Status.ToString(), Msg.Text);
            }
        }
    }
}
