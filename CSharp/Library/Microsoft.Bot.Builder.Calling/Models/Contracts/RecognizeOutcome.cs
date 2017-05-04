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
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the outcome of the "recognize" action. This is conveyed to the customer as POST to the customer CallBack Url.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class RecognizeOutcome : OperationOutcomeBase
    {
        /// <summary>
        /// Outcome of the Choice based recognition ( if any was specified in the action )
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public ChoiceOutcome ChoiceOutcome { get; set; }

        /// <summary>
        /// Outcome of the collectDigits recognition ( if any was specified in the action )
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public CollectDigitsOutcome CollectDigitsOutcome { get; set; }

        public RecognizeOutcome()
        {
            this.Type = ValidOutcomes.RecognizeOutcome;
        }

        public override void Validate()
        {
            base.Validate();
            if (this.Outcome == Outcome.Success)
            {
                bool choiceOutcome = (this.ChoiceOutcome != null);
                bool collectDigitsOutcome = (this.CollectDigitsOutcome != null);

                Utils.AssertArgument(
                    (choiceOutcome && !collectDigitsOutcome) || (!choiceOutcome && collectDigitsOutcome),
                    "Either a ChoiceOutcome or a CollectDigitsOutcome must for specified for successful recognition outcome");

                if (choiceOutcome)
                {
                    Utils.AssertArgument(!String.IsNullOrWhiteSpace(this.ChoiceOutcome.ChoiceName),
                        "Recognized choice name must be specified for successful choice recognition outcome");
                }

                if (collectDigitsOutcome)
                {
                    Utils.AssertArgument(!String.IsNullOrWhiteSpace(this.CollectDigitsOutcome.Digits),
                        "Collected digits must be specified for successful choice recognition outcome");
                    ValidDtmfs.Validate(this.CollectDigitsOutcome.Digits.ToCharArray());
                }
            }
        }
    }
}
