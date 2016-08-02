using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    public class ActivityTypes
    {
        /// <summary>
        /// Message from a user -> bot or bot -> User
        /// </summary>
        public const string Message = "message";

        /// <summary>
        ///  This notification is sent when the conversation's properties change, for example the topic name, or when user joins or leaves the group.
        /// </summary>
        public const string ConversationUpdate = "conversationUpdate";

        /// <summary>
        ///  Bot added or removed to contact list. See <see cref="ContactRelationUpdateActionTypes"/> for possible values.
        /// </summary>
        public const string ContactRelationUpdate = "contactRelationUpdate";

        /// <summary>
        /// A from is typing
        /// </summary>
        public const string Typing = "typing";

        /// <summary>
        /// Delete user data
        /// </summary>
        public const string DeleteUserData = "deleteUserData";

        /// <summary>
        /// Ping message
        /// </summary>
        public const string Ping = "ping";
    }
}