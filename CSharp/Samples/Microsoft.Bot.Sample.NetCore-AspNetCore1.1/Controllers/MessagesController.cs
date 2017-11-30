using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Sample.NetCore_AspNetCore1._1.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        //IConfiguarationRoot needs to be used here instead of IConfiguration like in .NET Core 2.0
        private readonly IConfigurationRoot configuration;
        public MessagesController(IConfigurationRoot configuration)
        {
            this.configuration = configuration;
        }

        [Authorize(Roles = "Bot")]
        [HttpPost]
        public async Task<OkResult> Post([FromBody] Activity activity)
        {
            var appCredentials = new MicrosoftAppCredentials(this.configuration);
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
