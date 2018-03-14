using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.FormFlowAttachmentsBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        internal static IDialog<ImagesForm> MakeRootDialog()
        {
            return Chain.From(() => FormDialog.FromForm(ImagesForm.BuildForm))
                .Do(async (context, form) =>
                {
                    try
                    {
                        var completed = await form;

                        // Actually process the images form..
                        await context.PostAsync("We are ready to process your images!");

                        // Storing your custom images..
                        var storageService = new StorageService();

                        var customImagesTextInfo = string.Empty;
                        var totalStorageRequired = default(long);
                        foreach (var image in completed.CustomImages)
                        {
                            using (var imageStream = await image)
                            {
                                var imageLength = await storageService.StoreImageAsync(imageStream);
                                customImagesTextInfo += $"{Environment.NewLine}- Name: '{image.Attachment.Name}' - Type: {image.Attachment.ContentType} - Size: {imageLength} bytes\n";
                                totalStorageRequired += imageLength;
                            }
                        }

                        await context.PostAsync($"We have processed your custom images - we have used {totalStorageRequired} bytes to store them: {customImagesTextInfo}");
                    }
                    catch (FormCanceledException<ImagesForm> e)
                    {
                        string reply;
                        if (e.InnerException == null)
                        {
                            reply = $"You quit on '{e.Last}'. Maybe you can finish next time!";
                        }
                        else
                        {
                            reply = "Sorry, I've had a short circuit. Please try again.";
                        }
                        await context.PostAsync(reply);
                    }
                });
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, MakeRootDialog);
            }
            else
            {
                HandleSystemMessage(activity);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}