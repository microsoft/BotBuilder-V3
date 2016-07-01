using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This concrete class defines the call state change notification schema.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class CallStateChangeNotification : NotificationBase
    {
        /// <summary>
        /// Call state types that will be used as part of call state change notification to the app.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public CallState CurrentState { get; set; }

        public CallStateChangeNotification()
        {
            this.Type = NotificationType.CallStateChange;
        }
    }
}
