using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// Message type for notifying agents that a user has added or removed
    /// them.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class AgentContactNotification : BaseMessage
    {
        public const string TypeName = "AgentContactNotification";
        public const string UserDisplayNameKey = "fromUserDisplayName";
        public const string LcidKey = "fromUserLcid";
        public const string ActionKey = "action";
        public const string ContactAddAction = "add";
        public const string ContactRemoveAction = "remove";

        public AgentContactNotification()
            : base(AgentContactNotification.TypeName)
        {
        }

        [JsonProperty(PropertyName = "fromUserDisplayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "fromUserLcid", Required = Required.Default)]
        public int Lcid { get; set; }

        [JsonProperty(PropertyName = "action", Required = Required.Always)]
        public string Action { get; set; }

        protected override void ValidateInternal()
        {
            VerifyPropertyExists(this.DisplayName, "DisplayName");
            VerifyPropertyExists(this.Action, "Action");
        }
    }
}
