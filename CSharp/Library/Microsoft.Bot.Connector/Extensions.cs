using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace Microsoft.Bot.Connector
{
    public static class Extensions
    {
        /// <summary>
        /// Take a message and create a reply message for it with the routing information 
        /// set up to correctly route a reply to the source message
        /// </summary>
        /// <param name="activity">the message being used to create the ReplyActivity from</param>
        /// <param name="text">text you want to reply with</param>
        /// <param name="locale">language of your reply</param>
        /// <returns>message set up to route back to the sender</returns>
        public static Activity CreateReply(this Activity activity, string text = null, string locale = null)
        {
            Activity reply = new Activity();
            reply.Type = ActivityTypes.Message;
            reply.Timestamp = DateTime.UtcNow;
            reply.From = activity.Recipient;
            reply.Recipient = activity.From;
            reply.Conversation = activity.Conversation;
            reply.Conversation.IsGroup = null; // don't need to send in a reply
            reply.Text = text ?? String.Empty;
            reply.Locale = locale ?? activity.Locale;
            return reply;
        }

        /// <summary>
        /// Check if the message has content
        /// </summary>
        /// <returns>Returns true if this message has any content to send</returns>
        public static bool HasContent(this Activity activity)
        {
            if (!String.IsNullOrWhiteSpace(activity.Text))
                return true;

            if (!String.IsNullOrWhiteSpace(activity.Summary))
                return true;

            if (activity.Attachments != null && activity.Attachments.Any())
                return true;

            if (activity.ChannelData != null)
                return true;

            return false;
        }


        /// <summary>
        /// Get channeldata as typed structure
        /// </summary>
        /// <typeparam name="TypeT">type to use</typeparam>
        /// <param name="activity">message</param>
        /// <returns>typed object or default(TypeT)</returns>
        public static TypeT GetChannelData<TypeT>(this Activity activity)
        {
            if (activity.ChannelData == null)
                return default(TypeT);
            return ((JObject)activity.ChannelData).ToObject<TypeT>();
        }

        /// <summary>
        /// Return the "major" portion of the activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>normalized major portion of the activity, aka message/... will return "message"</returns>
        public static string GetActivityType(this Activity activity)
        {
            var type = activity.Type.Split('/').First();
            if (String.Equals(type, ActivityTypes.Message, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.Message;

            if (String.Equals(type, ActivityTypes.ContactRelationUpdate, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.ContactRelationUpdate;

            if (String.Equals(type, ActivityTypes.ConversationUpdate, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.ConversationUpdate;

            if (String.Equals(type, ActivityTypes.DeleteUserData, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.DeleteUserData;

            if (String.Equals(type, ActivityTypes.Typing, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.Typing;

            return type;
        }

        /// <summary>
        /// Get a property from a BotData recorded retrieved using the REST API
        /// </summary>
        /// <param name="botData">BotData</param>
        /// <param name="property">property name to change</param>
        /// <returns>property requested or default for type</returns>
        public static TypeT GetProperty<TypeT>(this BotData botData, string property) 
        {
            if (botData?.Data == null)
                return default(TypeT);
            return GetPropertyData<TypeT>(botData.Data, property);
        }


        /// <summary>
        /// Set a property on a BotData record retrieved using the REST API
        /// </summary>
        /// <param name="botData">BotData</param>
        /// <param name="property">property name to change</param>
        /// <param name="data">new data</param>
        public static void SetProperty<TypeT>(this BotData botData, string property, TypeT data)
        {
            if (botData.Data == null)
                botData.Data = new JObject();
            SetPropertyData(botData.Data, property, data);
        }

        private static TypeT GetPropertyData<TypeT>(dynamic dynamicData, string property)
        {
            if (dynamicData?[property] == null)
                return default(TypeT);
            else if (typeof(TypeT) == typeof(byte[]))
                return (TypeT)(dynamic)Convert.FromBase64String((string)dynamicData?[property]);
            else if (typeof(TypeT).IsValueType)
                return (TypeT)dynamicData?[property];
            return dynamicData?[property]?.ToObject<TypeT>();
        }

        private static dynamic SetPropertyData(dynamic dynamicData, string property, object data)
        {
            if (data == null)
                dynamicData.Remove(property);
            else if (data is byte[])
                dynamicData[property] = Convert.ToBase64String((byte[])data);
            else if (data is string)
                dynamicData[property] = (string)data;
            else if (data.GetType().IsValueType)
                dynamicData[property] = JValue.FromObject(data);
            else if (data.GetType().IsArray)
                dynamicData[property] = JArray.FromObject(data);
            else
                dynamicData[property] = JObject.FromObject(data);
            return dynamicData;
        }
    }
}
