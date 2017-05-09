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
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// Base class for various actions
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public abstract class ActionBase
    {
        protected ActionBase()
            : this(isStandaloneAction: false)
        {
        }

        protected ActionBase(bool isStandaloneAction)
        {
            this.IsStandaloneAction = isStandaloneAction;
        }

        /// <summary>
        /// An operation Id needs to be specified by customer so that they can correlate outcome to the action.
        /// This becomes necessary when multiple actions are specified in one response body
        /// 
        /// Note: this is the first serialized field since it has the lowest order. By default Json.net starts 
        /// ordering from -1.
        /// </summary>
        [JsonProperty(Required = Required.Always, Order = -3)]
        public string OperationId { get; set; }

        /// <summary>
        /// The type of action. Various concrete action classes specify their name.
        /// This is used to deserialize a list of actions from JSON to their respective concrete classes.
        /// </summary>
        [JsonProperty(Required = Required.Always, Order = -2)]
        public string Action { get; set; }

        /// <summary>
        /// Flag to indicate whether this action must not be specified along with any other actions.
        /// </summary>
        [JsonIgnore]
        public bool IsStandaloneAction { get; private set; }

        /// <summary>
        /// similar to WCF IExtensibleDataObject, any data not expected on the wire is deserialized into this collection.
        /// </summary>
        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        public virtual void Validate()
        {
            Utils.AssertArgument(!String.IsNullOrWhiteSpace(this.OperationId), "A valid OperationId must be specified");            
        }
    }
}
