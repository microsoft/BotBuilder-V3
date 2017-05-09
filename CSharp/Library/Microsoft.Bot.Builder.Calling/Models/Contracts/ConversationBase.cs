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

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This base class defines a subset of properties which define a conversation.
    /// Conversation class derives from this and adds more properties - they are passed in OnIncomingCall
    /// ConversationResultBase class derives from this and adds more properties - they are passed in POST to callback Url to list operation outcomes
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public abstract class ConversationBase
    {
        /// <summary>
        /// Conversation Id 
        /// </summary>
        [JsonProperty(Required = Required.Always, Order = -2)]
        public string Id { get; set; }

        /// <summary>
        /// AppId of the customer ( if any )
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string AppId { get; set; }

        /// <summary>
        /// Opaque string to facilitate app developers to pass their custom data in this field. 
        /// This field is the same value that was passed 'response' by the customer.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string AppState { get; set; }

        /// <summary>
        /// Any links we want to surface to the customer for them to invoke us back on.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public Dictionary<string, Uri> Links { get; set; }

        public virtual void Validate()
        {
            Utils.AssertArgument(!String.IsNullOrWhiteSpace(this.Id), "Id cannot be null or empty");
            ApplicationState.Validate(this.AppState);
        }
    }
}
