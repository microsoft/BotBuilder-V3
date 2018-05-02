using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.BasicOAuth.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// This is the name of the OAuth Connection Setting that is configured for this bot
        /// </summary>
        private static string ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];

        private static IBotDataStore<BotData> DataStore = new InMemoryDataStore();

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            ConnectorClient client = new ConnectorClient(new Uri(activity.ServiceUrl));

            if (activity.Type == ActivityTypes.Message)
            {
                // Check if there is already a token for this user
                var oauthClient = activity.GetOAuthClient();
                var token = await oauthClient.OAuthApi.GetUserTokenAsync(activity.From.Id, ConnectionName).ConfigureAwait(false);

                if(token != null)
                {
                    var tokenReply = activity.CreateReply($"You are already signed in with token: {token.Token}");
                    await client.Conversations.ReplyToActivityAsync(tokenReply).ConfigureAwait(false);
                }
                else
                {
                    // see if the user is pending a login, if so, try to take the validation code and exchange it for a token
                    var data = await DataStore.LoadAsync(Address.FromActivity(activity), BotStoreType.BotPrivateConversationData, CancellationToken.None).ConfigureAwait(false);

                    if (!data.GetProperty<bool>("ActiveSignIn"))
                    {
                        // If the bot is not waiting for the user to sign in, ask them to do so
                        var tokenRequest = activity.CreateReply("Hello! Let's get you signed in!");
                        await client.Conversations.ReplyToActivityAsync(tokenRequest).ConfigureAwait(false);

                        // Send an OAuthCard to get the user signed in
                        var oauthReply = await activity.CreateOAuthReplyAsync(ConnectionName, "Please sign in", "Sign in").ConfigureAwait(false);
                        await client.Conversations.ReplyToActivityAsync(oauthReply).ConfigureAwait(false);

                        // Save some state saying an Active sign in is in progress for this user
                        data.SetProperty<bool>("ActiveSignIn", true);
                        await DataStore.SaveAsync(Address.FromActivity(activity), BotStoreType.BotPrivateConversationData, data, CancellationToken.None).ConfigureAwait(false);
                    }
                    else
                    {
                        await client.Conversations.ReplyToActivityAsync(activity.CreateReply("Let's see if that code works...")).ConfigureAwait(false);

                        // try to exchange the message text for a token
                        token = await oauthClient.OAuthApi.GetUserTokenAsync(activity.From.Id, ConnectionName, magicCode: activity.Text).ConfigureAwait(false);
                        if (token != null)
                        {
                            var tokenReply = activity.CreateReply($"It worked! You are now signed in with token: {token.Token}");
                            await client.Conversations.ReplyToActivityAsync(tokenReply).ConfigureAwait(false);

                            // The sign in is complete so set state to note that
                            data.SetProperty<bool>("ActiveSignIn", false);
                            await DataStore.SaveAsync(Address.FromActivity(activity), BotStoreType.BotPrivateConversationData, data, CancellationToken.None).ConfigureAwait(false);
                        }
                        else
                        {
                            var tokenReply = activity.CreateReply($"Hmm, that code wasn't right.");
                            await client.Conversations.ReplyToActivityAsync(tokenReply).ConfigureAwait(false);
                        }
                    }
                }
            }
            else if (activity.Type == ActivityTypes.Event)
            {
                if(activity.IsTokenResponseEvent())
                {
                    var token = activity.ReadTokenResponseContent();
                    var tokenReply = activity.CreateReply($"You are now signed in with token: {token.Token}");
                    await client.Conversations.ReplyToActivityAsync(tokenReply).ConfigureAwait(false);
                }
            }
            else
            {
                await HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task HandleSystemMessage(Activity message)
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
        }
    }
}
