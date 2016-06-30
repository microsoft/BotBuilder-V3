using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This base class defines a subset of properties which define a notification.
    /// CallStateNotification and RosterUpdates are examples of Notifications.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public abstract class NotificationBase : ConversationBase
    {
        /// <summary>
        /// Type of Notification
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public NotificationType Type { get; set; }
    }
}
