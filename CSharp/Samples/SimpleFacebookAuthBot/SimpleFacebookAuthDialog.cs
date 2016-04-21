using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Security.Authentication;

namespace Microsoft.Bot.Sample.SimpleFacebookAuthBot
{ 
    [Serializable]
    public class SimpleFacebookAuthDialog : IDialog<string>
    {

        public static readonly Uri FacebookOauthCallback = new Uri("http://localhost:4999/api/OAuthCallback");

        public static readonly string AuthTokenKey = "AuthToken";
        public readonly PendingMessage pendingMessage;

        public SimpleFacebookAuthDialog(Message msg)
        {
            pendingMessage = new PendingMessage(msg);
        }

       

        public static readonly IDialog<string> dialog = Chain
            .PostToChain()
            .Switch(
                new Case<Message, IDialog<string>>((msg) =>
                {
                    var regex = new Regex("^login", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    return regex.IsMatch(msg.Text);
                }, (ctx, msg) =>
                {
                    return Chain.ContinueWith(new SimpleFacebookAuthDialog(msg),
                                async (context, res) =>
                       {
                           var token = await res;
                           var valid = await FacebookHelpers.ValidAccessToken(token);
                           var name = await FacebookHelpers.GetFacebookProfileName(token);
                           context.UserData.SetValue("name", name);
                           return Chain.Return($"Your are logged in as: {name}");
                       });
                }),
                new Case<Message, IDialog<string>>((msg) =>
                {
                    var regex = new Regex("^logout", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    return regex.IsMatch(msg.Text);
                }, (ctx, msg) =>
                {
                    ctx.PerUserInConversationData.RemoveValue(AuthTokenKey);
                    ctx.UserData.RemoveValue("name");
                    return Chain.Return($"Your are logged out!");
                }),
                new DefaultCase<Message, IDialog<string>>((ctx, msg) =>
                {
                    string token;
                    string name = string.Empty; 
                    if (ctx.PerUserInConversationData.TryGetValue(AuthTokenKey, out token) && ctx.UserData.TryGetValue("name", out name))
                    {
                        var validationTask = FacebookHelpers.ValidAccessToken(token);
                        validationTask.Wait();
                        if (validationTask.IsCompleted && validationTask.Result)
                        {
                            return Chain.Return($"Your are logged in as: {name}");
                        }
                        else
                        {
                            return Chain.Return($"Your Token has been expired! Say \"login\" to log you back in!");
                        }
                    }
                    else
                    {
                        return Chain.Return("Say \"login\" when you want to login to facebook!");
                    }
                })
            ).Unwrap().PostToUser();


        public async Task StartAsync(IDialogContext context)
        {
            await InitLogIn(context);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var msg = await (argument);
            if (msg.Text.StartsWith("token:"))
            {
                var token = msg.Text.Remove(0, "token:".Length);
                context.PerUserInConversationData.SetValue(AuthTokenKey, token);
                context.Done(token);
            }
            else
            {
                await InitLogIn(context);
            }
        }

        private async Task InitLogIn(IDialogContext context)
        {
            string token;
            if (!context.PerUserInConversationData.TryGetValue(AuthTokenKey, out token))
            {
                context.PerUserInConversationData.SetValue("pendingMessage", pendingMessage);
                var fbLogin = $"Go to: {FacebookHelpers.GetFacebookLoginURL(pendingMessage, FacebookOauthCallback.ToString())}";

                await context.PostAsync(fbLogin);
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                context.Done(token);
            }
        }


       
      
    }
}