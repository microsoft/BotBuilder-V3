// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
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

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
    }

    public sealed class ConnectorClientFactory : IConnectorClientFactory
    {
        private readonly Uri serviceUri;
        private readonly IMessageActivity message; 
        private readonly MicrosoftAppCredentials credentials;
        private readonly bool? isEmulator; 
        public ConnectorClientFactory(IMessageActivity message, MicrosoftAppCredentials credentials)
        {
            SetField.NotNull(out this.message, nameof(message), message);
            SetField.NotNull(out this.credentials, nameof(credentials), credentials);
            SetField.CheckNull(nameof(message.ServiceUrl), message.ServiceUrl);

            this.serviceUri = new Uri(message.ServiceUrl);
            this.isEmulator = message.ChannelId?.Equals("emulator", StringComparison.OrdinalIgnoreCase);
        }

        IConnectorClient IConnectorClientFactory.MakeConnectorClient()
        {
            return new ConnectorClient(this.serviceUri, this.credentials);
        }

        IStateClient IConnectorClientFactory.MakeStateClient()
        {
            if(isEmulator ?? false)
            {
                // for emulator we should use serviceUri of the emulator for storage
                return new StateClient(this.serviceUri, this.credentials);
            }
            else
            {
                // TODO: remove this when going to against production
                //return new StateClient(new Uri("https://intercom-api-scratch.azurewebsites.net/"), this.credentials);
                return new StateClient(this.credentials);
            }
        }
    }
}
