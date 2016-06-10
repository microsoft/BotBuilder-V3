using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    public class ActivityTypes
    {
        public const string Message = "message";
        public const string MessageCarousel = "message/card.carousel";
        public const string MessageCard = "message/card";

        public const string ConversationUpdate = "conversationUpdate";
        public const string ContactRelationUpdate = "contactRelationUpdate";
        public const string Typing = "typing";
        public const string DeleteUserData = "deleteUserData";
    }
}