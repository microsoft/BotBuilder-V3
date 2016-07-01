using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This defines the set of the properties that define a rosterUpdate. 
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class RosterUpdateNotification : NotificationBase
    {
        /// <summary>
        /// List of participants in the conversation
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public IEnumerable<RosterParticipant> Participants { get; set; }

        public RosterUpdateNotification()
        {
            this.Type = NotificationType.RosterUpdate;
        }

        public override void Validate()
        {
            base.Validate();
            RosterParticipant.Validate(this.Participants);
        }
    }
}
