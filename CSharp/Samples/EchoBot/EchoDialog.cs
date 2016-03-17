using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.EchoBot
{
    public class EchoDialog : Dialog<string, bool>
    {
        public static readonly EchoDialog Instance = new EchoDialog();

        private EchoDialog()
            : base("echo")
        {
        }

        public override async Task<Connector.Message> ReplyReceivedAsync(ISession session)
        {
            var count = Convert.ToInt32(session.Stack.GetLocal("count"));
            session.Stack.SetLocal("count", count + 1);
            Connector.Message reply; 
            if (session.Message.Text == "reset")
            {
                reply = await Prompts.Confirm(session, "Are you sure you want to reset the count?",
                    "Didn't get that!");
            }
            else
            {
                reply = await session.CreateDialogResponse(string.Format("{0}: I heard {1}", count, session.Message.Text));
            }
            return reply; 
        }

        public override async Task<Connector.Message> DialogResumedAsync(ISession session, Task<bool> taskResult)
        {
            Connector.Message reply;
            if (taskResult.Status == TaskStatus.RanToCompletion)
            {
                var response = await taskResult;
                if (response)
                {
                    session.Stack.SetLocal("count", 0);
                    reply = await session.CreateDialogResponse("count reset.");
                }
                else
                {
                    reply = await session.CreateDialogResponse("Didn't reset.");
                }
            }
            else
            {
                reply = await session.CreateDialogResponse("ok");
            }

            return reply; 
        }
    }
}