using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This class defines a participant object within a rosterUpdate message
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class RosterParticipant
    {
        /// <summary>
        /// MRI of the participant
        /// </summary>
        [JsonProperty(Required = Required.Always, Order = -2)]
        public string Identity { get; set; }

        /// <summary>
        /// Participant Media Type . ex : audio
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public ModalityType MediaType { get; set; }

        /// <summary>
        /// Direction of media . ex : SendReceive
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string MediaStreamDirection { get; set; }

        public void Validate()
        {
            Utils.AssertArgument(!String.IsNullOrWhiteSpace(this.Identity), "Identity of participant must be specified");
            Utils.AssertArgument(!String.IsNullOrWhiteSpace(MediaStreamDirection), "MediaStreamDirection must be specified");
        }

        public static void Validate(IEnumerable<RosterParticipant> rosterParticipants)
        {
            Utils.AssertArgument(((rosterParticipants != null) && (rosterParticipants.Count<RosterParticipant>() > 0)), "Participant list cannot be null or empty");
            foreach (RosterParticipant participant in rosterParticipants)
            {
                Utils.AssertArgument(participant != null, "Participant cannot be null");
                participant.Validate();
            }
        }
    }
}
