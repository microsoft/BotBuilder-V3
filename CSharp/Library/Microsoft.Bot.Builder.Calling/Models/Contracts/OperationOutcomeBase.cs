using System;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// Base class for various "action(s)" outcome(s)
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public abstract class OperationOutcomeBase
    {
        /// <summary>
        /// The type of outcome. Various concrete outcome classes specify their name.
        /// This is used to deserialize (at the customer end) a list of outcomes from JSON to their respective concrete classes.
        /// </summary>
        [JsonProperty(Required = Required.Always, Order = -3)]
        public string Type { get; set; }

        /// <summary>
        /// The operation id which was specified when customer specified an action
        /// </summary>
        [JsonProperty(Required = Required.Always, Order = -2)]
        public string Id { get; set; }

        /// <summary>
        /// Outcome of the operation
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Outcome Outcome { get; set; }

        /// <summary>
        /// reason for failure (if any)
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string FailureReason { get; set; }

        public virtual void Validate()
        {
            Utils.AssertArgument(!String.IsNullOrWhiteSpace(this.Id), "Id cannot be null or empty");
            ValidOutcomes.Validate(this.Type);
        }
    }
}
