using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBot
{
    public enum StockOptions { Buy, Sell };

    [Serializable]
    public class StockOrder
    {
#pragma warning disable 1998
        public static IForm<StockOrder> MakeForm()
        {
            FormBuilder<StockOrder> _Order = new FormBuilder<StockOrder>();
            return _Order
                .Message("Welcome to the Stock Bot!")
                .Field("StockTicker", validate:
                async (state, value) =>
                {
                    ValidateResult vResult = new ValidateResult();
                    var str = value as string;

                    double? dblStock = await Yahoo.GetStockPriceAsync(str);
                    if (null == dblStock)
                    {
                        vResult.Feedback = string.Format("{0} is not a valid stock ticker.", str.ToUpper());
                        vResult.IsValid = false;
                    }
                    else
                    {
                        vResult.Feedback = string.Format("{0} is currently at {1}", str.ToUpper(), dblStock);
                        vResult.IsValid = true;
                    }

                    return vResult;
                })
                .Field(nameof(StockOrder.BuySell))
                .Field(nameof(StockOrder.OrderDate))
                .Field(nameof(StockOrder.NumberOfShares))
                .AddRemainingFields()
                .Confirm("Do you want to {BuySell} {NumberOfShares} shares of {StockTicker} on {OrderDate}? ")
                .OnCompletionAsync(async (session, StockOrder) =>
                {
                    Debug.WriteLine("{0}", StockOrder);
                })
                .Build();
        }


        [Prompt("Which Stock Ticker are you interested in? {||}")]
        [Describe("Stock Ticker, example: MSFT")]
        public string StockTicker;

        [Prompt("Do you want to Buy or Sell? {||}")]
        [Describe("Buy or Sell stock")]
        public StockOptions? BuySell;

        [Prompt("How many shares do you want to buy or sell? {||}")]
        [Describe("Number of shares to buy or sell")]
        public int NumberOfShares;

        [Prompt("What date would you like this transaction to take place? {||}")]
        [Describe("Date of Transaction")]
        public DateTime OrderDate;
    };
}
