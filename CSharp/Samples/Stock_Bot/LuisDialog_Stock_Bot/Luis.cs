using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockLuisDlg
{
    [LuisModel("9fa4985b-351f-4e5e-8c8f-b726795a98b4", "a5d38008ca0f4e2187314509e3671953")]
    [Serializable]
    public class StockDialog : LuisDialog<object>
    {
        [LuisIntent("StockPrice")]
        public async Task StockPrice(IDialogContext context, LuisResult result)
        {
            string strRet = result.Entities[0].Entity;
            context.ConversationData.SetValue<string>("LastStock", strRet);

            await context.PostAsync(await YahooStock.Yahoo.GetStock(strRet));
            context.Wait(MessageReceived);
        }

        [LuisIntent("RepeatLastStock")]
        public async Task RepeatLastStock(IDialogContext context, LuisResult result)
        {
            string strRet = string.Empty;
            string strStock = string.Empty;
            if (!context.ConversationData.TryGetValue("LastStock", out strStock))
            {
                strRet = "I don't have a previous stock to look up!";
            }
            else
            {
                strRet = await YahooStock.Yahoo.GetStock(strStock);
            }
            await context.PostAsync(strRet);
            context.Wait(MessageReceived);
        }

        [LuisIntent("None")]
        public async Task NoneHandler(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm sorry, I don't understand");
            context.Wait(MessageReceived);
        }
    }
}