using System;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using AdaptiveCards;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.Bot.Sample.AadV2Bot.Dialogs
{
    /// <summary>
    /// This Dialog enables the user to issue a set of commands against AAD
    /// to do things like list recent email, send an email, and identify the user
    /// This Dialog also makes use of the GetTokenDialog to help the user login
    /// </summary>
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        /// <summary>
        /// This is the name of the OAuth Connection Setting that is configured for this bot
        /// </summary>
        private static string ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        /// <summary>
        /// Supports the commands recents, send, me, and signout against the Graph API
        /// </summary>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            
            var message = activity.Text.ToLowerInvariant();
            
            if (message.ToLowerInvariant().Equals("recents"))
            {
                // Display recent emails from the Graph API
                context.Call(CreateGetTokenDialog(), ListRecentMail);
            }
            else if (message.ToLowerInvariant().StartsWith("send"))
            {
                // Send an email using the Graph API from the logged in user
                var recipient = message.ToLowerInvariant().Split(' ');
                if (recipient.Length == 2)
                {
                    context.Call(CreateGetTokenDialog(), async (IDialogContext ctx, IAwaitable<GetTokenResponse> tokenResponse) => {
                        await SendMail(context, tokenResponse, recipient[1]);
                    });
                }
                else
                {
                    await context.PostAsync("You need to enter: 'send <recipient_email>' to send an email.");
                }
            }
            else if (message.ToLowerInvariant().Equals("me"))
            {
                // Display information about the logged in user
                context.Call(CreateGetTokenDialog(), ListMe);
            }
            else if (message.ToLowerInvariant().Equals("signout"))
            {
                // Sign the user out from AAD
                await Signout(context);
            }
            else
            {
                await context.PostAsync("You can type 'recents', 'send <recipient_email>', or 'me' to list things from AAD v2.");
                context.Wait(MessageReceivedAsync);
            }
        }

        /// <summary>
        /// Signs the user out from AAD
        /// </summary>
        public static async Task Signout(IDialogContext context)
        {
            await context.SignOutUserAsync(ConnectionName);
            await context.PostAsync($"You have been signed out.");
        }

        /// <summary>
        /// Creates a GetTokenDialog using custom strings
        /// </summary>
        private GetTokenDialog CreateGetTokenDialog()
        {
            return new GetTokenDialog(
                ConnectionName, 
                $"Please sign in to {ConnectionName} to proceed.",
                "Sign In",
                2,
                "Hmm. Something went wrong, let's try again.");                
        }

        #region AAD Tasks

        private async Task ListRecentMail(IDialogContext context, IAwaitable<GetTokenResponse> tokenResponse)
        {
            var token = await tokenResponse;
            var client = new SimpleGraphClient(token.Token);

            var card = new AdaptiveCard();
            var container = new Container();
            card.Body.Add(container);
            container.Items.Add(new TextBlock()
            {
                Text = "Here are all of your unread Inbox emails from the last 30 minutes:",
                Weight = TextWeight.Bolder,
                Size = TextSize.Medium,
                Wrap = true
            });
            container.Items.Add(new TextBlock()
            {
                Text = " ",
                Weight = TextWeight.Bolder,
                Size = TextSize.Medium
            });
            var messages = await client.GetRecentUnreadMail();
            int i = 1;
            foreach (var m in messages)
            {
                var messageContainer = new Container();
                messageContainer.Items.Add(new TextBlock() { Text = m.From.EmailAddress.Name, Weight = TextWeight.Bolder, Size = TextSize.Medium });
                messageContainer.Items.Add(new TextBlock() { Text = $"_{m.Subject}_", Wrap = true });
                messageContainer.Items.Add(new TextBlock() { Text = m.BodyPreview, Wrap = true });
                messageContainer.Items.Add(new TextBlock() { Text = " " });
                container.Items.Add(messageContainer);
            }

            var repoMessage = context.MakeMessage();
            if (repoMessage.Attachments == null)
            {
                repoMessage.Attachments = new List<Attachment>();
            }
            repoMessage.Attachments.Add(new Attachment()
            {
                Content = card,
                ContentType = "application/vnd.microsoft.card.adaptive",
                Name = "Repositories"
            });
            await context.PostAsync(repoMessage);
        }

        private async Task SendMail(IDialogContext context, IAwaitable<GetTokenResponse> tokenResponse, string recipient)
        {
            var token = await tokenResponse;
            var client = new SimpleGraphClient(token.Token);

            var me = await client.GetMe();

            await client.SendMail(recipient, "Message from a bot!", $"Hi there! I had this message sent from a bot. - Your friend, {me.DisplayName}");

            await context.PostAsync($"I sent a message to '{recipient}' from your account :)");
        }


        private async Task ListMe(IDialogContext context, IAwaitable<GetTokenResponse> tokenResponse)
        {
            var token = await tokenResponse;
            var client = new SimpleGraphClient(token.Token);

            var me = await client.GetMe();
            var manager = await client.GetManager();

            await context.PostAsync($"You are {me.DisplayName} and you report to {manager.DisplayName}.");
        }
        #endregion
    }
}
