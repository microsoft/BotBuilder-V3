using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This class describes a participant.
    /// This can be a participant in any modality in a 2 or multi-party conversation
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Participant
    {
        /// <summary>
        /// MRI of the participant .ex : 2:+14258828080 or '8:alice' 
        /// </summary>
        [JsonProperty(Required = Required.Always, Order = -2)]
        public string Identity { get; set; }

        /// <summary>
        /// Display name of participant if received from the controllers
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Participant language. This property is optional and only passed if
        /// participant language is known.
        /// Examples of valid values are null, "en", "en-US".
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string LanguageId { get; set; }

        /// <summary>
        /// Is this participant the originator of the conversation
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public bool Originator { get; set; }

        public void Validate()
        {
            Utils.AssertArgument(!String.IsNullOrWhiteSpace(this.Identity), "Identity of participant must be specified");
        }

        public static void Validate(IEnumerable<Participant> participants)
        {
            Utils.AssertArgument(participants != null, "participant list cannot be null");
            Utils.AssertArgument(participants.Any(), "participant list cannot be empty");
            foreach (Participant participant in participants)
            {
                Utils.AssertArgument(participant != null, "participant cannot be null");
                participant.Validate();
            }
        }
    }
}
