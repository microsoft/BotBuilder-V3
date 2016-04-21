using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Autofac;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace Microsoft.Bot.Sample.SimpleFacebookAuthBot.Controllers
{
    public class OAuthCallbackController : ApiController
    {
        private static Lazy<string> botId = new Lazy<string>(() => ConfigurationManager.AppSettings["AppId"]);

        /// <summary>
        /// OAuth call back that is called by Faceboo. Read https://developers.facebook.com/docs/facebook-login/manually-build-a-login-flow for more details.
        /// </summary>
        /// <param name="userId"> The Id for the user that is getting authenticated.</param>
        /// <param name="conversationId"> The Id of the conversation.</param>
        /// <param name="code"> The Authentication code returned by Facebook.</param>
        /// <param name="state"> The state returned by Facebook.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/OAuthCallback")]
        public async Task<HttpResponseMessage> OAuthCallback([FromUri] string userId, [FromUri] string conversationId, [FromUri] string code, [FromUri] string state)
        {

            // Check if the bot is running against emulator
            var connectorType = HttpContext.Current.Request.IsLocal ? ConnectorType.Emulator : ConnectorType.Cloud;

            // Exchange the Facebook Auth code with Access toekn
            var token = await FacebookHelpers.ExchangeCodeForAccessToken(userId, conversationId, code, SimpleFacebookAuthDialog.FacebookOauthCallback.ToString());

            // Create the message that is send to conversation to resume the login flow
            var msg = new Message
            {
                Text = $"token:{token.AccessToken}",
                From = new ChannelAccount { Id = userId },
                To = new ChannelAccount { Id = botId.Value },
                ConversationId = conversationId
            };

            // Resume the conversation to SimpleFacebookAuthDialog
            var reply = await Conversation.ResumeAsync(botId.Value, userId, conversationId, msg, connectorType: connectorType);

            // Remove the pending message because login flow is complete
            IBotData dataBag = new JObjectBotData(reply);
            PendingMessage pending;
            if (dataBag.PerUserInConversationData.TryGetValue("pendingMessage", out pending))
            {
                dataBag.PerUserInConversationData.RemoveValue("pendingMessage");
                var pendingMessage = pending.GetMessage();
                reply.To = pendingMessage.From;
                reply.From = pendingMessage.To;

                // Send the login success asynchronously to user
                var client = Conversation.ResumeContainer.Resolve<IConnectorClient>(TypedParameter.From(connectorType));
                await client.Messages.SendMessageAsync(reply);

                return Request.CreateResponse("You are now logged in! Continue talking to the bot.");
            }
            else
            {
                // Callback is called with no pending message as a result the login flow cannot be resumed.
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new InvalidOperationException("Cannot resume!"));
            }
        }
    }
}
