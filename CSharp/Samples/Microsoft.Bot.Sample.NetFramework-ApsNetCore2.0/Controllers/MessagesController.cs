using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Sample.NetFramework_ApsNetCore2._0.Controllers
{
    [BotAuthentication]
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IConfiguration configuration;

        public MessagesController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // POST api/values
        [HttpPost]
        public async Task<OkResult> Post([FromBody] Activity activity)
        {
            var appCredentials = new MicrosoftAppCredentials(
                configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value,
                configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value);
            var client = new ConnectorClient(new Uri(activity.ServiceUrl), appCredentials);
            var reply = activity.CreateReply();
            if (activity.Type == ActivityTypes.Message)
            {
                reply.Text = $"echo: {activity.Text}";
            }
            else
            {
                reply.Text = $"activity type: {activity.Type}";
            }
            await client.Conversations.ReplyToActivityAsync(reply);
            return Ok();
        }
    }
}
