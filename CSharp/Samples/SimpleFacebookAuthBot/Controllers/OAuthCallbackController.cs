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

        [HttpGet]
        [Route("api/OAuthCallback")]
        public async Task<HttpResponseMessage> OAuthCallback([FromUri] string userId, [FromUri] string conversationId, [FromUri] string code, [FromUri] string state)
        {

            var connectorType = HttpContext.Current.Request.IsLocal ? ConnectorType.Emulator : ConnectorType.Cloud;

            var authToken = await FacebookHelpers.ExchangeCodeForAuthToken(userId, conversationId, code, SimpleFacebookAuthDialog.FacebookOauthCallback.ToString());

            var msg = new Message
            {
                Text = $"token:{authToken.AccessToken}",
                From = new ChannelAccount { Id = userId },
                To = new ChannelAccount { Id = botId.Value },
                ConversationId = conversationId
            };

            var reply = await Conversation.ResumeAsync(botId.Value, userId, conversationId, msg, connectorType: connectorType);

            IBotData dataBag = new JObjectBotData(reply);
            PendingMessage pending;
            if (dataBag.PerUserInConversationData.TryGetValue("pendingMessage", out pending))
            {
                dataBag.PerUserInConversationData.RemoveValue("pendingMessage");
                var pendingMessage = pending.GetMessage();
                reply.To = pendingMessage.From;
                reply.From = pendingMessage.To;

                var client = Conversation.ResumeContainer.Resolve<IConnectorClient>(TypedParameter.From(connectorType));
                await client.Messages.SendMessageAsync(reply);

                return Request.CreateResponse("You are now logged in! Continue talking to the bot.");
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new InvalidOperationException("Cannot resume!"));
            }
            

            
        }
    }
}
