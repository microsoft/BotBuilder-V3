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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    /// <summary>
    /// Factory for IConnectorClient.
    /// </summary>
    public interface IConnectorClientFactory
    {
        /// <summary>
        /// Make the IConnectorClient implementation.
        /// </summary>
        /// <returns>The IConnectorClient implementation.</returns>
        IConnectorClient MakeConnectorClient();

        /// <summary>
        /// Make the <see cref="IStateClient"/> implementation.
        /// </summary>
        /// <returns>The <see cref="IStateClient"/> implementation.</returns>
        IStateClient MakeStateClient();

        /// <summary>
        /// Make the <see cref="IOAuthClient"/> implementation.
        /// </summary>
        /// <returns>The <see cref="IOAuthClient"/> implementation.</returns>
        IOAuthClient MakeOAuthClient();
    }

    public sealed class ConnectorClientFactory : IConnectorClientFactory
    {
        private readonly Uri serviceUri;
        private readonly IAddress address;
        private readonly MicrosoftAppCredentials credentials;
        private readonly ConnectorClient connectorClient;
        private readonly StateClient stateClient;
        private readonly OAuthClient oauthClient;

        // NOTE: These should be moved to autofac registration
        private static readonly ConcurrentDictionary<string, ConnectorClient> connectorClients = new ConcurrentDictionary<string, ConnectorClient>();
        private static readonly ConcurrentDictionary<string, StateClient> stateClients = new ConcurrentDictionary<string, StateClient>();
        private static readonly ConcurrentDictionary<string, OAuthClient> oauthClients = new ConcurrentDictionary<string, OAuthClient>();

        public ConnectorClientFactory(IAddress address, MicrosoftAppCredentials credentials)
        {
            SetField.NotNull(out this.address, nameof(address), address);
            SetField.NotNull(out this.credentials, nameof(credentials), credentials);

            this.serviceUri = new Uri(address.ServiceUrl);
            string key = $"{serviceUri}{credentials.MicrosoftAppId}";
            if (!connectorClients.TryGetValue(key, out connectorClient))
            {
                connectorClient = new ConnectorClient(this.serviceUri, this.credentials);
                connectorClients[key] = connectorClient;
            }

            if (!stateClients.TryGetValue(key, out stateClient))
            {
                if (IsEmulator(this.address))
                {
                    // for emulator we should use serviceUri of the emulator for storage
                    stateClient = new StateClient(this.serviceUri, this.credentials);
                }
                else
                {
                    if (!string.IsNullOrEmpty(settingsStateApiUrl.Value))
                    {
                        stateClient = new StateClient(new Uri(settingsStateApiUrl.Value), this.credentials);
                    }
                    else
                    {
                        stateClient = new StateClient(this.credentials);
                    }
                }
                stateClients[key] = stateClient;
            }

            if (!oauthClients.TryGetValue(key, out oauthClient))
            {
                // only create the oauthclient if we have credentials
                if (!String.IsNullOrEmpty(credentials?.MicrosoftAppId) && !String.IsNullOrEmpty(credentials?.MicrosoftAppPassword))
                {
                    if (IsEmulator(this.address) && emulateOAuthCards.Value)
                    {
                        // for emulator using emulated OAuthCards we should use serviceUri of the emulator
                        oauthClient = new OAuthClient(this.serviceUri, this.credentials);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(settingsOAuthApiUrl.Value))
                        {
                            oauthClient = new OAuthClient(new Uri(settingsOAuthApiUrl.Value), this.credentials);
                        }
                        else
                        {
                            oauthClient = new OAuthClient(this.credentials);
                        }
                    }

                    if (IsEmulator(this.address))
                    {
                        // Send the mode notification (emulated OAuthCards or not) to the emulator
                        Task.Run(async () => await oauthClient.OAuthApi.SendEmulateOAuthCardsAsync(emulateOAuthCards.Value).ConfigureAwait(false)).Wait();
                    }

                    oauthClients[key] = oauthClient;
                }
            }
        }

        public static bool IsEmulator(IAddress address)
        {
            return address.ChannelId.Equals(ChannelIds.Emulator, StringComparison.OrdinalIgnoreCase);
        }

        IConnectorClient IConnectorClientFactory.MakeConnectorClient()
        {
            return connectorClient;
        }

        IStateClient IConnectorClientFactory.MakeStateClient()
        {
            return stateClient;
        }

        IOAuthClient IConnectorClientFactory.MakeOAuthClient()
        {
            return oauthClient;
        }

        private readonly static Lazy<string> settingsStateApiUrl = new Lazy<string>(() => GetSettingsStateApiUrl());

        /// <summary>
        /// Get the state api endpoint from settings. 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The state api endpoint from settings.</returns>
        private static string GetSettingsStateApiUrl(string key = "BotStateEndpoint")
        {
            var url = SettingsUtils.GetAppSettings(key);
            if (!string.IsNullOrEmpty(url))
            {
                MicrosoftAppCredentials.TrustServiceUrl(url, DateTime.MaxValue);
            }
            return url;
        }

        private readonly static Lazy<string> settingsOAuthApiUrl = new Lazy<string>(() => GetOAuthApiFromSettingsUrl());

        /// <summary>
        /// Get the OAuth API endpoint from settings. 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The OAuth API endpoint from settings.</returns>
        private static string GetOAuthApiFromSettingsUrl(string key = "OAuthApiEndpoint")
        {
            var url = SettingsUtils.GetAppSettings(key);
            if (!string.IsNullOrEmpty(url))
            {
                MicrosoftAppCredentials.TrustServiceUrl(url, DateTime.MaxValue);
            }
            return url;
        }

        private readonly static Lazy<bool> emulateOAuthCards = new Lazy<bool>(() => GetEmulateOAuthCardsSetting());

        private static bool GetEmulateOAuthCardsSetting(string key = "EmulateOAuthCards")
        {
            var value = SettingsUtils.GetAppSettings(key);

            bool result = false;
            if (!string.IsNullOrEmpty(value) && !bool.TryParse(value, out result))
            {
                // default back to false
                result = false;
            }

            return result;
        }
    }
}
