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

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the action which customers can specify to indicate that the server call agent should accept the call.
    /// The media is hosted by the server call agent
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Answer : ActionBase
    {
        public static readonly IEnumerable<ModalityType> DefaultAcceptModalityTypes = new ModalityType[] { ModalityType.Audio };

        private IEnumerable<ModalityType> acceptModalityTypes;

        /// <summary>
        /// The modality types the application will accept.  If none are specified,
        /// audio-only is assumed.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public IEnumerable<ModalityType> AcceptModalityTypes
        {
            get
            {
                if (this.acceptModalityTypes == null || !this.acceptModalityTypes.Any())
                {
                    return DefaultAcceptModalityTypes;
                }
                else
                {
                    return this.acceptModalityTypes;
                }
            }

            set
            {
                this.acceptModalityTypes = value;
            }
        }

        public Answer()
        {
            this.Action = ValidActions.AnswerAction;
        }

        public override void Validate()
        {
            base.Validate();
            Utils.AssertArgument(this.Action == ValidActions.AnswerAction, "Action was not Answer");
            Utils.AssertArgument(this.AcceptModalityTypes.Distinct().Count() == this.AcceptModalityTypes.Count(), "AcceptModalityTypes cannot contain duplicate elements.");
            Utils.AssertArgument(this.AcceptModalityTypes.All((m) => { return m != ModalityType.Unknown; }), "AcceptModalityTypes contains an unknown media type.");
            Utils.AssertArgument(this.AcceptModalityTypes.All((m) => { return m != ModalityType.VideoBasedScreenSharing; }), "AcceptModalityTypes cannot contain VideoBasedScreenSharing.");
        }
    }
}
