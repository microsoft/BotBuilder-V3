using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is a part of the "recognize" action outcome. This is specified if the customer had specified any collectDigits operation.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class CollectDigitsOutcome
    {
        /// <summary>
        /// Completion reason
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public DigitCollectionCompletionReason CompletionReason { get; set; }

        /// <summary>
        /// Digits collected ( if any )
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string Digits { get; set; }
    }
}
