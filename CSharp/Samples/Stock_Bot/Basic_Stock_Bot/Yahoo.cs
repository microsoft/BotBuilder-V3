using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace YahooStock
{
    class Yahoo
    {
        public static async Task<string> GetStock(string strStock)
        {
            string strRet = string.Empty;
            double? dblStock = await Yahoo.GetStockPriceAsync(strStock);

            if (null == dblStock)   // might be a company name rather than a stock ticker name
            {
                string strTicker = await GetStockTickerName(strStock);
                if (string.Empty != strTicker)
                {
                    dblStock = await Yahoo.GetStockPriceAsync(strTicker);
                    strStock = strTicker;
                }
            }

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

        private static async Task<double?> GetStockPriceAsync(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return null;

            string url = $"http://finance.yahoo.com/d/quotes.csv?s={symbol}&f=sl1";
            string csv;
            using (WebClient client = new WebClient())
            {
                csv = await client.DownloadStringTaskAsync(url).ConfigureAwait(false);
            }
            string line = csv.Split('\n')[0];
            string price = line.Split(',')[1];

            double result;
            if (double.TryParse(price, out result))
                return result;

            return null;
        }

        private static async Task<string> GetStockTickerName(string strCompanyName)
        {
            string strRet = string.Empty;
            string url = $"http://d.yimg.com/autoc.finance.yahoo.com/autoc?query={strCompanyName}&region=1&lang=en&callback=YAHOO.Finance.SymbolSuggest.ssCallback";
            string sJson = string.Empty;
            using (WebClient client = new WebClient())
            {
                sJson = await client.DownloadStringTaskAsync(url).ConfigureAwait(false);
            }

            sJson = StripJsonString(sJson);
            YhooCompanyLookup lookup = null;
            try
            {
                lookup = JsonConvert.DeserializeObject<YhooCompanyLookup>(sJson);
            }
            catch (Exception e)
            {

            }

            if (null != lookup)
            {
                foreach (lResult r in lookup.ResultSet.Result)
                {
                    if (r.exch == "NAS")
                    {
                        strRet = r.symbol;
                        break;
                    }
                }
            }

            return strRet;
        }

        // String retrurned from Yahoo Company name lookup contains more than raw JSON
        // strip off the front/back to get to raw JSON
        private static string StripJsonString(string sJson)
        {
            int iPos = sJson.IndexOf('(');
            if (-1 != iPos)
            {
                sJson = sJson.Substring(iPos + 1);
            }

            iPos = sJson.LastIndexOf(')');
            if (-1 != iPos)
            {
                sJson = sJson.Substring(0, iPos);
            }

            return sJson;
        }
    }

    public class lResult
    {
        public string symbol { get; set; }
        public string name { get; set; }
        public string exch { get; set; }
        public string type { get; set; }
        public string exchDisp { get; set; }
        public string typeDisp { get; set; }
    }

    public class ResultSet
    {
        public string Query { get; set; }
        public lResult[] Result { get; set; }
    }

    public class YhooCompanyLookup
    {
        public ResultSet ResultSet { get; set; }
    }

}
