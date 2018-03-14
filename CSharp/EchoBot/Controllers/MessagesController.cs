using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace EchoBot.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {

        public async Task Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                MicrosoftAppCredentials.AutoTokenRefreshTimeSpan = TimeSpan.FromSeconds(30);

                ConnectorClient client = new ConnectorClient(new Uri(activity.ServiceUrl), new MicrosoftAppCredentials());
                await client.Conversations.ReplyToActivityAsync(activity.CreateReply(activity.Text));
            }
        }
    }
}
