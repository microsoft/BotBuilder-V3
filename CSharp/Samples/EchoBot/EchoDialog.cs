using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.EchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        private int count;

        public async Task StartAsync(IDialogContext context, IAwaitable<object> argument)
        {
            context.Wait(MessageReceived);
        }

        public async Task MessageReceived(IDialogContext context, IAwaitable<Message> argument)
        {
            var message = await argument;
            if (message.Text == "reset")
            {
                Prompts.Confirm(
                    context,
                    AfterConfirmReset,
                    "Are you sure you want to reset the count?",
                    "Didn't get that!");
            }
            else
            {
                var text = string.Format("{0}: I heard {1}", this.count, message.Text);
                await context.PostAsync(text);

                context.Wait(MessageReceived);
            }

            ++this.count;
        }

        public async Task AfterConfirmReset(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                this.count = 0;
                await context.PostAsync("count reset.");
            }
            else
            {
                await context.PostAsync("did not reset count.");
            }

            context.Wait(MessageReceived);
        }
    }
}