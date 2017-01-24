using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Sample.EchoBot
{
    [Serializable]
    public class EchoLocationDialog : EchoDialog
    {
        public override async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = await argument;
            if (msg.Text.ToLower() == "location")
            {
                await context.Forward(new FacebookLocationDialog(), ResumeAfter, msg, CancellationToken.None);
            }
            else
            {
                await base.MessageReceivedAsync(context, argument);
            }
        }

        public async Task ResumeAfter(IDialogContext context, IAwaitable<Place> result)
        {
            var place = await result;

            if (place != default(Place))
            {
                var geo = (place.Geo as JObject)?.ToObject<GeoCoordinates>();
                if (geo != null)
                {
                    var reply = context.MakeMessage();
                    reply.Attachments.Add(new HeroCard
                    {
                        Title = "Open your location in bing maps!",
                        Buttons = new List<CardAction> {
                            new CardAction
                            {
                                Title = "Your location",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.bing.com/maps/?v=2&cp={geo.Latitude}~{geo.Longitude}&lvl=16&dir=0&sty=c&sp=point.{geo.Latitude}_{geo.Longitude}_You%20are%20here&ignoreoptin=1"
                            }
                        }

                    }.ToAttachment());

                    await context.PostAsync(reply);
                }
                else
                {
                    await context.PostAsync("No GeoCoordinates!");
                }
            }
            else
            {
                await context.PostAsync("No location extracted!");
            }

            context.Wait(MessageReceivedAsync);
        }
    }

    [Serializable]
    public class FacebookLocationDialog : IDialog<Place>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = await argument;
            if (msg.ChannelId == "facebook")
            {
                var reply = context.MakeMessage();
                reply.ChannelData = new FacebookMessage
                (
                    text: "Please share your location with me.",
                    quickReplies: new List<FacebookQuickReply>
                    {
                        // If content_type is location, title and payload are not used
                        // see https://developers.facebook.com/docs/messenger-platform/send-api-reference/quick-replies#fields
                        // for more information.
                        new FacebookQuickReply(
                            contentType: FacebookQuickReply.ContentTypes.Location,
                            title: default(string),
                            payload: default(string)
                        )
                    }
                );
                await context.PostAsync(reply);
                context.Wait(LocationReceivedAsync);
            }
            else
            {
                context.Done(default(Place));
            }
        }

        public virtual async Task LocationReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var msg = await argument;
            var location = msg.Entities?.Where(t => t.Type == "Place").Select(t => t.GetAs<Place>()).FirstOrDefault();
            context.Done(location);
        }
    }
}