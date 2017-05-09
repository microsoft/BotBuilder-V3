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
    /// This is the action which customers can specify to indicate that the server call agent should start recording user speech.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Record : ActionBase
    {
        /// <summary>
        /// Promt to played out (if any) before recording starts. 
        /// Customers can choose to specify "playPrompt" action separately or 
        /// specify as part of "record" - mostly all recordings are preceeded by a prompt
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public PlayPrompt PlayPrompt { get; set; }

        /// <summary>
        /// Maximum duration of recording . Default : 180 secs
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public double? MaxDurationInSeconds { get; set; }

        /// <summary>
        /// Maximum initial silence allowed from the time we start the recording operation 
        /// before we timeout and fail the operation. 
        /// 
        /// Default : 5
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public double? InitialSilenceTimeoutInSeconds { get; set; }

        /// <summary>
        /// Maximum allowed silence once the user has started speaking before we conclude 
        /// the user is done recording.
        /// 
        /// Default : 1
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public double? MaxSilenceTimeoutInSeconds { get; set; }

        /// <summary>
        /// The format is which the recording is expected.
        /// 
        /// Default : wma
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public RecordingFormat? RecordingFormat { get; set; }

        /// <summary>
        /// If specified "true", then we would play a beep before starting recording operation
        /// 
        /// Default : true
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public bool? PlayBeep { get; set; }

        /// <summary>
        /// Stop patterns which users can punch to end recording. 
        /// 
        /// Ex: " Press pound when done recording" 
        /// Or "Press 11 when done recording".
        /// 
        /// Note: each stop tone is a string, since the application might 
        /// potentially want to stop recording based on when user presses # or 11.
        /// Thus multiple digits together might constitute a single stop tone pattern. 
        /// Hence it is represented as a string and not an int.
        /// 
        /// Default : none
        /// 
        /// TODO : change to string from char later when scenario emerges
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public IEnumerable<char> StopTones { get; set; }

        public Record()
        {
            this.Action = ValidActions.RecordAction;
        }

        public override void Validate()
        {
            base.Validate();
            Utils.AssertArgument(this.Action == ValidActions.RecordAction, "Action was not Record");

            if (this.PlayPrompt != null)
            {
                this.PlayPrompt.Validate();
            }

            if (this.StopTones != null)
            {
                ValidDtmfs.Validate(this.StopTones);
            }

            if (this.MaxDurationInSeconds.HasValue)
            {
                Utils.AssertArgument(this.MaxDurationInSeconds.Value >= MinValues.RecordingDuration.TotalSeconds && this.MaxDurationInSeconds.Value <= MaxValues.RecordingDuration.TotalSeconds,
                    "MaxDurationInSeconds has to be specified in the range of {0} - {1} secs", MinValues.RecordingDuration.TotalSeconds, MaxValues.RecordingDuration.TotalSeconds);
            }

            if (this.InitialSilenceTimeoutInSeconds.HasValue)
            {
                Utils.AssertArgument(this.InitialSilenceTimeoutInSeconds.Value >= MinValues.InitialSilenceTimeout.TotalSeconds && this.InitialSilenceTimeoutInSeconds.Value <= MaxValues.InitialSilenceTimeout.TotalSeconds,
                    "InitialSilenceTimeoutInSeconds has to be specified in the range of {0} - {1} secs", MinValues.InitialSilenceTimeout.TotalSeconds, MaxValues.InitialSilenceTimeout.TotalSeconds);
            }

            if (this.MaxSilenceTimeoutInSeconds.HasValue)
            {
                Utils.AssertArgument(this.MaxSilenceTimeoutInSeconds.Value >= MinValues.SilenceTimeout.TotalSeconds && this.MaxSilenceTimeoutInSeconds.Value <= MaxValues.SilenceTimeout.TotalSeconds,
                    "MaxSilenceTimeoutInSeconds has to be specified in the range of {0} - {1} secs", MinValues.SilenceTimeout.TotalSeconds, MaxValues.SilenceTimeout.TotalSeconds);
            }
        }
    }
}
