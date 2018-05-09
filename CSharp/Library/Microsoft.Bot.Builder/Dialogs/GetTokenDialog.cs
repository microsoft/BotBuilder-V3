// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A Dialog to rerieve a user token for a configured OAuth connection
    /// This Dialog will first attempt to rerieve the user token from the Azure Bot Service
    /// If the Azure Bot Service does not already have a token, the GetTokenDialog will send
    /// the user an OAuthCard.
    /// The GetTokenDialog will then wait for either the user to come back, or for the user to send
    /// a validation code. The Dialog will attempt to exchange whatever response is sent for the 
    /// user token. If successful, the dialog will return the token and otherwise will retry the
    /// specified number of times.
    /// </summary>
    [Serializable]
    public class GetTokenDialog : IDialog<GetTokenResponse>
    {
        private string _connectionName;
        private string _buttonLabel;
        private string _signInMessage;
        private int _reties;
        private string _retryMessage;

        public GetTokenDialog(string connectionName, string signInMessage, string buttonLabel, int retries = 0, string retryMessage = null)
        {
            _connectionName = connectionName;
            _signInMessage = signInMessage;
            _buttonLabel = buttonLabel;
            _reties = retries;
            _retryMessage = retryMessage;
        }

        public async Task StartAsync(IDialogContext context)
        {
            // First ask Bot Service if it already has a token for this user
            var token = await context.GetUserTokenAsync(_connectionName);
            if (token != null)
            {
                context.Done(new GetTokenResponse() { Token = token.Token });
            }
            else
            {
                // If Bot Service does not have a token, send an OAuth card to sign in
                await SendOAuthCardAsync(context, (Activity)context.Activity);
            }
        }

        private async Task SendOAuthCardAsync(IDialogContext context, Activity activity)
        {
            var reply = await activity.CreateOAuthReplyAsync(_connectionName, _signInMessage, _buttonLabel);
            await context.PostAsync(reply);
            context.Wait(WaitForToken);
        }

        private async Task WaitForToken(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var tokenResponse = activity.ReadTokenResponseContent();
            string verificationCode = null;
            if (tokenResponse != null)
            {
                context.Done(new GetTokenResponse() { Token = tokenResponse.Token });
                return;
            }
            else if(activity.IsTeamsVerificationInvoke())
            {
                JObject value = activity.Value as JObject;
                if (value != null)
                {
                    verificationCode = (string)(value["state"]);
                }
            }
            else if (!string.IsNullOrEmpty(activity.Text))
            {
                verificationCode = activity.Text;
            }

            tokenResponse = await context.GetUserTokenAsync(_connectionName, verificationCode);
            if (tokenResponse != null)
            {
                context.Done(new GetTokenResponse() { Token = tokenResponse.Token });
                return;
            }
            
            // decide whether to retry or not
            if (_reties > 0)
            {
                _reties--;
                await context.PostAsync(_retryMessage);
                await SendOAuthCardAsync(context, activity);
            }
            else
            {
                context.Done(new GetTokenResponse() { NonTokenResponse = activity.Text });
                return;
            }
        }
    }

    /// <summary>
    /// Result object from the GetTokenDialog
    /// If the GetToken action is successful in retrieving a user token, the GetTokenDialog will be populated with the Token property
    /// If the GetToken action is unsuccessful in retrieving a user token, the GetTokenDialog will be populated with the NonTokenResponse property
    /// </summary>
    public class GetTokenResponse
    {
        /// <summary>
        /// The user token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The text that the user typed when the GetTokenDialog is unable to retrieve a user token
        /// </summary>
        public string NonTokenResponse { get; set; }
    }
}
