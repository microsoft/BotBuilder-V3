using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json;
using Microsoft.Bot.Builder;

namespace Microsoft.Bot.Sample.SimpleAlarmBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and reply to it
        /// </summary>
        [ResponseType(typeof(Message))]
        public async Task<HttpResponseMessage> Post([FromBody]Message message)
        {
            return await CompositionRoot.SendAsync(this.Request, message, () => new SimpleAlarmBot());
        }

        // ------  to send a message 
        // ConnectorClient botConnector = new BotConnector();
        // ... use message.CreateReplyMessage() to create a message, or
        // ... create a new message and set From, To, Text 
        // await botConnector.Messages.SendMessageAsync(message);
    }
}