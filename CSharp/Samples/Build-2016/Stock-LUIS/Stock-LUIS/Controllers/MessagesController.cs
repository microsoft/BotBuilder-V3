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

namespace Stock_LUIS
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                bool bSetStock = false;
                StockLUIS stLuis = await LUISStockClient.ParseUserInput(message.Text);
                string strRet = string.Empty;
                string strStock = message.Text;

                if (stLuis.intents.Count() > 0)
                {
                    switch (stLuis.intents[0].intent)
                    {
                        case "RepeatLastStock":
                            strStock = message.GetBotUserData<string>("LastStock");
                            if (null == strStock)
                            {
                                strRet = "I don't have a previous stock to look up!";
                            }
                            else
                            {
                                strRet = await GetStock(strStock);
                            }
                            break;
                        case "StockPrice":
                            bSetStock = true;
                            strRet = await GetStock(stLuis.entities[0].entity);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    strRet = "Sorry, I don't understand...";
                }


                Message ReplyMessage = message.CreateReplyMessage(strRet);
                if (bSetStock)
                {
                    ReplyMessage.SetBotUserData("LastStock", stLuis.entities[0].entity);
                }
                return ReplyMessage;
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