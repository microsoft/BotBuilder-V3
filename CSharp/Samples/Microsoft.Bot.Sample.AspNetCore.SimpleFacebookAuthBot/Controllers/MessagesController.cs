using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Sample.AspNetCore.SimpleFacebookAuthBot.Controllers
{
    [Route("api/[controller]")]
    [BotAuthentication]
    public class MessagesController : Controller
    {

        private readonly ILogger<MessagesController> logger;

        public MessagesController(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<MessagesController>();
        }

        // POST api/values
        [HttpPost]
        public virtual async Task<IActionResult> Post([FromBody]Activity activity)
        {
            if (activity != null)
            {
                // one of these will have an interface and process it
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        await Conversation.SendAsync(activity, () => SimpleFacebookAuthDialog.dialog);
                        break;

                    case ActivityTypes.ConversationUpdate:
                    case ActivityTypes.ContactRelationUpdate:
                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                    default:
                        logger.LogError($"Unknown activity type ignored: {activity.GetActivityType()}");
                        break;
                }
            }
            return StatusCode((int) HttpStatusCode.Accepted);
        }
    }
}
