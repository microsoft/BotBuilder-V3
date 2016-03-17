using Microsoft.Bot.Connector;
using System;
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

        protected static ConsoleReplyMessage Map(Task<Connector.Message> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    return new ConsoleReplyMessage() { Status = HttpStatusCode.OK, Msg = task.Result };
                case TaskStatus.Faulted:
                    var error = task.Exception.InnerException as HttpException;
                    var status = error != null ? (HttpStatusCode)error.GetHttpCode() : HttpStatusCode.InternalServerError;
                    return new ConsoleReplyMessage() { Status = status };
                case TaskStatus.Canceled:
                    return new ConsoleReplyMessage() { Status = HttpStatusCode.OK };
                default:
                    throw new NotImplementedException();
            }
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
