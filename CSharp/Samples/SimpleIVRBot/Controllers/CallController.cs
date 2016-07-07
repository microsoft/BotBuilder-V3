using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Connector;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Bot.Sample.SimpleIVRBot
{

    [BotAuthentication]
    [RoutePrefix("api/calling")]
    public class CallingController : ApiController
    {
        public CallingController()
            : base()
        {
            CallingConversation.RegisterCallingBot(c => new SimpleIVRBot(c));
        }

        [Route("callback")]
        public async Task<HttpResponseMessage> ProcessCallingEventAsync()
        {
            return await CallingConversation.SendAsync(Request, CallRequestType.CallingEvent);
        }

        [Route("call")]
        public async Task<HttpResponseMessage> ProcessIncomingCallAsync()
        {
            return await CallingConversation.SendAsync(Request, CallRequestType.IncomingCall);
        }
    }
}