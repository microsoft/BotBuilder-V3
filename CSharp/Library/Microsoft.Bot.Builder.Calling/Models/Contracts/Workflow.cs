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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This class contains the workflow the customer sent for the OnInComingCall POST or any subsequent POST to their callback url.
    /// Basically this workflow defines the set of actions, the customer wants us to perform and then callback to them.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Workflow
    {
        /// <summary>
        /// Callback link to call back the customer on, once we have performed the set of actions.
        /// 
        /// Note : 
        /// a. We would always return the outcome :
        ///     i. of the last operation if all operations were performed successfully OR
        ///     ii. outcome of first failed operation 
        /// b. If any operation fails, then we immediately callback the customer webservice with the outcome, 
        ///     and skip processing other operations defined in the "actions" list. 
        /// c. If no callback link is provided, then we keep performing all specified operations, until 
        ///     i. we hit the end - then we hangup (if call connected to server call agent)
        ///     ii. We hit a failure - then we hangup (if call connected to server call agent)
        ///     iii. We hit a max call duration timeout - then we hangup (if call connected to server call agent)
        /// d. Any validation failure of this workflow object would result in us returning
        ///    the workflowValidationOutcome object to the customer's callback url and not proceed with any defined actions.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public CallbackLink Links { get; set; }

        /// <summary>
        /// List of actions to perform . ex : playPrompt, record, hangup
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public IEnumerable<ActionBase> Actions { get; set; }

        /// <summary>
        /// Opaque string to facilitate app developers to pass their custom data in this field. 
        /// This field is echo'd back in the 'result' POST for this 'workflow'.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string AppState { get; set; }

        /// <summary>
        /// Validate the WorkFlow
        /// </summary>
        public void Validate()
        {
            this.Validate(expectEmptyActions: false);
        }

        /// <summary>
        /// Validate the WorkFlow
        /// </summary>
        /// <param name="expectEmptyActions">Allow Actions to be empty</param>
        public virtual void Validate(bool expectEmptyActions)
        {
            if (expectEmptyActions)
            {
                Utils.AssertArgument(this.Actions == null || this.Actions.Count() == 0, "Actions must either be null or empty collection");
            }
            else
            {
                ValidActions.Validate(this.Actions);
            }

            if (this.Links != null)
            {
                Utils.AssertArgument(this.Links.Callback != null, "Callback link cannot be specified as null");
                Utils.AssertArgument(this.Links.Callback.IsAbsoluteUri, "Callback link must be an absolute uri");
                Utils.AssertArgument(this.Links.Callback.Scheme == "https", "Callback link must be an secure HTTPS uri");
            }

            ApplicationState.Validate(this.AppState);
        }
    }
}
