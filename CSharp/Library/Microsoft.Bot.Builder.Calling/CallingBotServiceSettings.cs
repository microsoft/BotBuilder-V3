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

using Microsoft.Bot.Builder.Calling.Exceptions;
using System;
using System.Configuration;

namespace Microsoft.Bot.Builder.Calling
{
    public class CallingBotServiceSettings
    {
        /// <summary>
        /// The url where the Callingcallbacks from Skype Bot platform will be sent. Needs to match the domain name of service and the route configured in BotController.
        /// For example "https://testservice.azurewebsites.net/api/calling/callback"   
        /// </summary>
        public string CallbackUrl { get; set; }

        /// <summary>
        /// Loads core bot library configuration from the cloud service configuration
        /// </summary>
        /// <returns>MessagingBotServiceSettings</returns>
        public static CallingBotServiceSettings LoadFromCloudConfiguration()
        {
            CallingBotServiceSettings settings;

            try
            {
                settings = new CallingBotServiceSettings
                {
                    CallbackUrl = ConfigurationManager.AppSettings.Get("Microsoft.Bot.Builder.Calling.CallbackUrl")
                };
            }
            catch (Exception e)
            {
                throw new BotConfigurationException(
                    "A mandatory configuration item is missing or invalid", e);
            }

            settings.Validate();
            return settings;
        }

        /// <summary>
        ///     Validates current bot configuration and throws BotConfigurationException if the configuration is invalid
        /// </summary>
        public void Validate()
        {
            Uri callBackUri;
            if (!Uri.TryCreate(this.CallbackUrl, UriKind.Absolute, out callBackUri))
            {
                throw new BotConfigurationException($"Bot calling configuration is invalid, callback url: {CallbackUrl} is not a valid url!");
            }
        }
    }
}
