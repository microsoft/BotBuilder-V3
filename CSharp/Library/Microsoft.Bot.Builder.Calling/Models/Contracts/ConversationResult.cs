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

using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// Once we have peformed the "actions" requested by the customer, we POST back to customer callback Url with this "result" object.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class ConversationResult : ConversationBase
    {
        /// <summary>
        /// a. We would always return the outcome :
        ///     i. of the last operation if all operations were performed successfully OR
        ///     ii. outcome of first failed operation 
        /// b. If any operation fails, then we immediately callback the customer webservice with the outcome, 
        ///     and skip processing other operations defined in the "actions" list. 
        /// c. If no callback link is provided, then we keep performing all specified operations, until 
        ///     i. we hit the end - then we hangup (if call connected to server call agent)
        ///     ii. We hit a failure - then we hangup (if call connected to server call agent)
        ///     iii. We hit a max call duration timeout - then we hangup (if call connected to server call agent)
        /// d. Any validation failure of this response object would result in us returning
        ///    the WorkflowValidationOutcome object to the customer's callback url and not proceed with any defined actions.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public OperationOutcomeBase OperationOutcome { get; set; }

        /// <summary>
        /// Current state of the Call
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public CallState CallState { get; set; }

        public override void Validate()
        {
            base.Validate();
            ValidOutcomes.Validate(this.OperationOutcome);
        }
    }
}
