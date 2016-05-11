using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.EchoBot
{
    [Serializable]
    public class EchoAttachmentDialog : EchoDialog
    {
        public override async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var message = await argument;
            if (message.Text.ToLower() == "makeattachment")
            {
                var reply = context.MakeMessage();
                reply.Text = string.Format("{0}: You said {1}", this.count++, message.Text);

                reply.Attachments = new List<Attachment>();

                var actions = new List<Microsoft.Bot.Connector.Action>();
                for (int i = 0; i < 3; i++)
                {
                    actions.Add(new Microsoft.Bot.Connector.Action
                    {
                        Title = $"Button:{i}",
                        Message = $"Action:{i}"
                    });
                }

                for (int i = 0; i < 10; i++)
                {
                    reply.Attachments.Add(new Attachment
                    {
                        Title = $"title{i}",
                        ContentType = "image/jpeg",
                        ContentUrl = $"https://placeholdit.imgix.net/~text?txtsize=35&txt=image{i}&w=120&h=120",
                        Actions = actions
                    });
                }
                await context.PostAsync(reply);
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                await base.MessageReceivedAsync(context, argument);
            }
        }
    }
}