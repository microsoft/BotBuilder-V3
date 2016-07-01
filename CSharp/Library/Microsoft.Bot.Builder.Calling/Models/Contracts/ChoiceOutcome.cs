using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is a part of the "recognize" action outcome. This is specified if the customer had specified any recognition options.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class ChoiceOutcome
    {
        /// <summary>
        /// Completion reason of the recognition operation
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public RecognitionCompletionReason CompletionReason { get; set; }

        /// <summary>
        /// Choice that was recognized (if any)
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string ChoiceName { get; set; }
    }
}
