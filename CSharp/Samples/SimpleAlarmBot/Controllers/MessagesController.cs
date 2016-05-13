using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Bot.Sample.SimpleAlarmBot
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
            return await Conversation.SendAsync(message, () => new SimpleAlarmDialog());
        }

        // ------  to send a message 
        // ConnectorClient botConnector = new BotConnector();
        // ... use message.CreateReplyMessage() to create a message, or
        // ... create a new message and set From, To, Text 
        // await botConnector.Messages.SendMessageAsync(message);
    }
}