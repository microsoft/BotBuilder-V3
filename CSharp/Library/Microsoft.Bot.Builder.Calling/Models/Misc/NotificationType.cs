using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// List of various notification types 
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<NotificationType>))]
    public enum NotificationType
    {
        /// <summary>
        /// Not recognized notification type.
        /// </summary>
        Unknown,

        /// <summary>
        /// Roster Update Notification
        /// </summary>
        RosterUpdate,

        /// <summary>
        /// Call State change notification
        /// </summary>
        CallStateChange
    }
}
