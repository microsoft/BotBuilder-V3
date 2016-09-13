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
        public override async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            if (message.Text.ToLower() == "makeattachment")
            {
                var reply = context.MakeMessage();
                reply.Text = string.Format("{0}: You said {1}", this.count++, message.Text);

                reply.Attachments = new List<Attachment>();

                var actions = new List<CardAction>();
                for (int i = 0; i < 3; i++)
                {
                    actions.Add(new CardAction
                    {
                        Title = $"Button:{i}",
                        Value = $"Action:{i}",
                        Type = ActionTypes.ImBack
                    });
                }
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                for (int i = 0; i < 5; i++)
                {
                    reply.Attachments.Add(
                         new HeroCard
                         {
                             Title = $"title{i}",
                             Images = new List<CardImage>
                            {
                                new CardImage
                                {
                                    Url = $"https://placeholdit.imgix.net/~text?txtsize=35&txt=image{i}&w=120&h=120"
                                }
                            },
                             Buttons = actions
                         }.ToAttachment()
                    );
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