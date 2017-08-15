using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net;
using System.Dynamic;
using System.IO;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Persistent_Menu_Facebook.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string BASE_URI = "https://graph.facebook.com/v2.6/me/messenger_profile?";
        private static string PAGE_ACCESS_TOKEN = WebConfigurationManager.AppSettings["FacebookAccessToken"];
        private static bool isAdmin = true; //ToDo: Implement a way to tell an admin apart from user like asking them to paste the Page Access Token. 

        public enum Options
        {
            GetStarted, ActivateMenu, ShowMenu, DeleteMenu
        };

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            Object _result = null;
            string Data = string.Empty;
            Options option;
            if (Enum.TryParse(activity.Text, out option) && isAdmin)
            {
                // (Admin) Activate / desactivate the permanent menu, set the Get Started button at the start of the conversation or show the content of the permanent menu.
                switch (option)
                {
                    case Options.ActivateMenu:
                        TextReader tr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "PersistentMenu.json");
                        Data = tr.ReadToEnd();
                        _result = HttpRequestHelper(BASE_URI + "access_token=" + PAGE_ACCESS_TOKEN, "POST", Data);
                        break;

                    case Options.ShowMenu:
                        _result = HttpRequestHelper(BASE_URI + "fields=persistent_menu&access_token=" + PAGE_ACCESS_TOKEN, "GET", null);
                        break;

                    case Options.DeleteMenu:
                        Data = "{'fields':['persistent_menu']}";
                        _result = HttpRequestHelper(BASE_URI + "access_token=" + PAGE_ACCESS_TOKEN, "DELETE", Data);
                        break;

                    case Options.GetStarted:
                        dynamic JsonData = new ExpandoObject();
                        JsonData.get_started = new ExpandoObject();
                        JsonData.get_started.payload = "GET_STARTED_PAYLOAD";
                        _result = HttpRequestHelper(BASE_URI + "access_token=" + PAGE_ACCESS_TOKEN, "POST", JsonData);
                        break;

                    default:
                        break;
                }
                await context.PostAsync($"{_result}");
            }
            else
            {
                // Manage Postback requests from Messeger
                switch (activity.Text)
                {
                    case "PAYBILL_PAYLOAD":
                        //DoSomething();
                        await context.PostAsync($"The user clicked 'Pay Bill' in the permanent menu");
                        break;

                    case "HISTORY_PAYLOAD":
                        //DoSomething();
                        await context.PostAsync($"The user clicked 'History' in the permanent menu");
                        break;

                    case "CONTACT_INFO_PAYLOAD":
                        //DoSomething();
                        await context.PostAsync($"The user clicked 'Contact Info' in the permanent menu");
                        break;

                    default:
                        if (isAdmin)
                        {
                            string optionsString = string.Empty;
                            foreach (Options op in EnumUtil.GetValues<Options>()) { optionsString += op + ", "; };
                            await context.PostAsync($"You sent {activity.Text}. The available options are:  {optionsString}");
                        }
                        else
                            await context.PostAsync($"Sorry I didn't understand your question");
                        break;
                }
            }
            context.Wait(MessageReceivedAsync);
        }

        private static Object HttpRequestHelper(string Uri, string Method, dynamic JsonData)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Uri);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = Method;
            string Data = string.Empty;

            if (!Method.Equals("GET"))
            {
                using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(JsonData);
                }
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                Data = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject(Data);
        }
        public static class EnumUtil
        {
            public static IEnumerable<T> GetValues<T>()
            {
                return Enum.GetValues(typeof(T)).Cast<T>();
            }
        }
    }
}