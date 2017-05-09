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
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the action which customers can specify to indicate that the server call agent should perform speech or dtmf recognition.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Recognize : ActionBase
    {
        /// <summary>
        /// Promt to played out (if any) before recognition starts. 
        /// Customers can choose to specify "playPrompt" action separately or 
        /// specify as part of "recognize" - mostly all recognitions are preceeded by a prompt
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public PlayPrompt PlayPrompt { get; set; }

        /// <summary>
        /// Are customers allowed to enter choice before prompt finishes. Default : true.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public bool? BargeInAllowed { get; set; }

        /// <summary>
        /// Culture of Speech Recognizer to use. Default : en-US.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public Culture? Culture { get; set; }

        /// <summary>
        /// Maximum initial silence allowed from the time we start the recognition operation 
        /// before we timeout and fail the operation. 
        /// 
        /// if we are playing a prompt, then this timer starts after prompt finishes.
        /// 
        /// Default : 5 secs
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public double? InitialSilenceTimeoutInSeconds { get; set; }

        /// <summary>
        /// Mamimum allowed time between digits if we are doing dtmf based choice recognition or CollectDigits recognition
        /// 
        /// Default : 1 sec
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public double? InterdigitTimeoutInSeconds { get; set; }

        /// <summary>
        /// List of choices to recognize against. Choices can be speech or dtmf based.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public IEnumerable<RecognitionOption> Choices { get; set; }

        /// <summary>
        /// There is no choice based recognition. Rather collect all digits entered by user.
        /// 
        /// Either CollectDigits or Choices must be specified. Both can not be specified.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public CollectDigits CollectDigits { get; set; }

        public Recognize()
        {
            this.Action = ValidActions.RecognizeAction;
        }

        public override void Validate()
        {
            base.Validate();
            Utils.AssertArgument(this.Action == ValidActions.RecognizeAction, "Action was not Recognize");

            if (this.PlayPrompt != null)
            {
                this.PlayPrompt.Validate();
            }

            if (this.InitialSilenceTimeoutInSeconds.HasValue)
            {
                Utils.AssertArgument(this.InitialSilenceTimeoutInSeconds.Value >= MinValues.InitialSilenceTimeout.TotalSeconds && this.InitialSilenceTimeoutInSeconds.Value <= MaxValues.InitialSilenceTimeout.TotalSeconds,
                    "InitialSilenceTimeoutinSeconds has to be specified in the range of {0} - {1} secs", MinValues.InitialSilenceTimeout.TotalSeconds, MaxValues.InitialSilenceTimeout.TotalSeconds);
            }

            if (this.InterdigitTimeoutInSeconds.HasValue)
            {
                Utils.AssertArgument(this.InterdigitTimeoutInSeconds.Value >= MinValues.InterdigitTimeout.TotalSeconds && this.InterdigitTimeoutInSeconds.Value <= MaxValues.InterDigitTimeout.TotalSeconds,
                    "InterDigitTimeoutInSeconds has to be specified in the range of {0} - {1} secs", MinValues.InterdigitTimeout.TotalSeconds, MaxValues.InterDigitTimeout.TotalSeconds);
            }

            bool choicesSpecified = (this.Choices != null);
            bool collectDigitsSpecified = (this.CollectDigits != null);

            Utils.AssertArgument(
                (choicesSpecified && !collectDigitsSpecified) || (collectDigitsSpecified && !choicesSpecified),
                "Either of RecognitionOption or CollectDigits must be specified");

            if (choicesSpecified)
            {
                RecognitionOption.Validate(this.Choices);
            }
            else if (collectDigitsSpecified)
            {
                this.CollectDigits.Validate();
            }
        }
    }
}
