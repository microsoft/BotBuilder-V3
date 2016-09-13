using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This class represents a single prompt
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Prompt
    {
        /// <summary>
        /// This can be :
        /// 1) Text - specifying the text to be tts'd
        /// 2) Empty if just silence to be played out
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string Value { get; set; }

        /// <summary>
        /// Uri of any media file to be played out
        /// </summary>
        public Uri FileUri { get; set; }

        /// <summary>
        /// The voice to use to tts out if "value" is text. Default : Male
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public VoiceGender? Voice { get; set; }

        /// <summary>
        /// The culture to use to tts out if "value" is text. Default : en-US
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public Culture? Culture { get; set; }

        /// <summary>
        /// Any silence to be played out before playing "value". 
        /// If "value" is null - this field must be a valid > 0 value.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public uint? SilenceLengthInMilliseconds { get; set; }

        /// <summary>
        /// Whether to emphasize when tts'ing out - if "value" is text. Default : false
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public bool? Emphasize { get; set; }

        /// <summary>
        /// Whether to customize pronunciation when tts'ing out - if "value" is text. Default : none
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public SayAs? SayAs { get; set; }

        public void Validate()
        {
            bool fileSpecified = (this.FileUri != null);
            bool textSpecified = !String.IsNullOrWhiteSpace(this.Value);
            Utils.AssertArgument(
                fileSpecified || textSpecified || this.SilenceLengthInMilliseconds.GetValueOrDefault() > 0,
                "Either a prompt file/text has to be specified or a valid period of silence has to be specified or both can be specified");
            Utils.AssertArgument(!(fileSpecified && textSpecified), "Both a prompt file and a TTS text cannot be specified at the same time.");

            if (textSpecified)
            {
                Utils.AssertArgument(this.Value.Length <= MaxValues.LengthOfTTSText, "Length of text to be TTS'd has to be smaller than {0} characters", MaxValues.LengthOfTTSText);
            }

            if (this.SilenceLengthInMilliseconds.HasValue)
            {
                Utils.AssertArgument(this.SilenceLengthInMilliseconds.Value <= MaxValues.SilentPromptDuration.TotalMilliseconds,
                    "SilenceLengthInMilliSeconds has to be specified in the range of 0 - {0} msecs", MaxValues.SilentPromptDuration.TotalMilliseconds);
            }
        }

        public static void Validate(IEnumerable<Prompt> prompts)
        {
            Utils.AssertArgument(prompts != null, "Prompts list cannot be null");
            Utils.AssertArgument(prompts.Any(), "Prompts list cannot be empty");
            foreach (Prompt prompt in prompts)
            {
                Utils.AssertArgument(prompt != null, "Prompt cannot be null");
                prompt.Validate();
            }
        }
    }
}
