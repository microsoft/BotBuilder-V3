using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.EchoBot
{
    public class EchoDialog : Dialog<string, PromptDialogResult<bool>>
    {
        public static readonly EchoDialog Instance = new EchoDialog();

        private EchoDialog()
            : base("echo")
        {
        }

        public override async Task<DialogResponse> ReplyReceivedAsync(ISession session)
        {
            var count = Convert.ToInt32(session.Stack.GetDialogState("count"));
            session.Stack.SetDialogState("count", count + 1);
            DialogResponse reply = null; 
            if (session.Message.Text == "reset")
            {
                reply =  await Prompts.Confirm(session, "Are you sure you want to reset the count?",
                    "Didn't get that!");
            }
            else
            {
                reply = await session.CreateDialogResponse(string.Format("{0}: I heard {1}", count, session.Message.Text));
            }
            return reply; 
        }

        public override async Task<DialogResponse> DialogResumedAsync(ISession session, PromptDialogResult<bool> result)
        {
            DialogResponse reply = null;
            if (result != null && result.Completed)
            {
                if (result.Response)
                {
                    session.Stack.SetDialogState<int>("count", 0);
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