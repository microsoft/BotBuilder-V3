using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;

namespace Microsoft.Bot.Sample.EchoBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            //return await Conversation.SendAsync(message, () => new EchoDialog());
            //return await Conversation.SendAsync(message, () => EchoCommandDialog.dialog);
            return await Conversation.SendAsync(message, () => EchoChainDialog.dialog);
        }
    }
}
