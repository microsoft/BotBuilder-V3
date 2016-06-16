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
                reply.Type = ActivityTypes.MessageCarousel;
                reply.Text = string.Format("{0}: You said {1}", this.count++, message.Text);

                reply.Attachments = new List<Attachment>();

                var actions = new List<Microsoft.Bot.Connector.Action>();
                for (int i = 0; i < 3; i++)
                {
                    actions.Add(new Microsoft.Bot.Connector.Action
                    {
                        Title = $"Button:{i}",
                        Value = $"Action:{i}", 
                        Type = "postBack"
                    });
                }

                
                for (int i = 0; i < 10; i++)
                {

                    reply.Attachments.Add(new Attachment
                    {
                        ContentType = "application/vnd.microsoft.card.hero",
                        Content = new HeroCard
                        {
                            Title = $"title{i}",
                            Images = new List<Image>
                            {
                                new Image
                                {
                                    Url = $"https://placeholdit.imgix.net/~text?txtsize=35&txt=image{i}&w=120&h=120"
                                }
                            }, 
                            Buttons = actions
                        }
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