using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;

namespace StockBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        internal static IFormDialog<StockOrder> MakeRoot()
        {
            return FormDialog.FromForm(StockOrder.MakeForm);
        }

        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                return await Conversation.SendAsync(message, MakeRoot);
            }
            else
            {
                return HandleSystemMessage(message);
            }
        }
        private async Task<string> GetStock(string strStock)
        {
            string strRet = string.Empty;
            double? dblStock = await Yahoo.GetStockPriceAsync(strStock);
            // return our reply to the user
            if (null == dblStock)
            {
                strRet = string.Format("Stock {0} doesn't appear to be valid", strStock.ToUpper());
            }
            else
            {
                strRet = string.Format("Stock: {0}, Value: {1}", strStock.ToUpper(), dblStock);
            }

            return strRet;
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
                return message.CreateReplyMessage("Hello Botty McBotFace!");
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }
}