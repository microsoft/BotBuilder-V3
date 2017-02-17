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

using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Capability for a specific channel
    /// </summary>
    public interface IChannelCapability
    {
        /// <summary>
        /// True if the channel support buttons, false otherwise.
        /// </summary>
        bool SupportButtons { get; }
    }

    /// <summary>
    /// Channel capability detector.
    /// </summary>
    public interface IDetectChannelCapability
    {
        /// <summary>
        /// Detects channel capabilities.
        /// </summary>
        /// <returns>
        /// Capabilities of a channel.
        /// </returns>
        IChannelCapability Detect();
    }

    public sealed class DetectChannelCapability : IDetectChannelCapability
    {
        private readonly IAddress address;

        public DetectChannelCapability(IAddress address)
        {
            SetField.NotNull(out this.address, nameof(address), address);
        }

        public IChannelCapability Detect()
        {
            var isEmulator = ConnectorClientFactory.IsEmulator(this.address);
            var capability = new ChannelCapability(supportButtons: ! isEmulator);
            return capability;
        }
    }

    public sealed class ChannelCapability : IChannelCapability
    {
        private readonly bool supportButtons;

        public ChannelCapability(bool supportButtons = true)
        {
            this.supportButtons = supportButtons;
        }

        public bool SupportButtons
        {
            get
            {
                return supportButtons;
            }
        }
    }
}
