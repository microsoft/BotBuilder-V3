using System;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// By default Json.net doesn't know how to deserialize JSON data into Interfaces or abstract classes.
    /// This custom Converter helps deserialize "Notifications" specified in JSON into respective concrete "Notification" classes.
    /// </summary>
    public class NotificationConverter : JsonCreationConverter<NotificationBase>
    {
        protected override NotificationBase Create(Type objectType, JObject jsonObject)
        {
            var type = (string)jsonObject.Property("type");
            if (String.Equals(type, NotificationType.CallStateChange.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return new CallStateChangeNotification();
            }
            else
            {
                throw new ArgumentException(String.Format("The given notification type '{0}' is not supported!", type));
            }
        }
    }
}
