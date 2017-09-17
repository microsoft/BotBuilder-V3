using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.AspNetCore.SimpleIVRBot.Controllers
{

    [BotAuthentication]
    [Route("api/calling")]
    public class CallingController : Controller
    {
        public CallingController()
            : base()
        {
            CallingConversation.RegisterCallingBot(c => new SimpleIVRBot(c));
        }

        [Route("callback")]
        public async Task<IActionResult> ProcessCallingEventAsync()
        {
            return await CallingConversation.SendAsync(Request, CallRequestType.CallingEvent);
        }

        [Route("")]         // In case of someone missed the "call"
        [Route("call")]
        public async Task<IActionResult> ProcessIncomingCallAsync()
        {
            return await CallingConversation.SendAsync(Request, CallRequestType.IncomingCall);
        }
    }
}