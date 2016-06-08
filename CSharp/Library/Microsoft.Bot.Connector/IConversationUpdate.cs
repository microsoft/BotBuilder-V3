using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// The Properties of a conversation are different
    /// </summary>
    public interface IConversationUpdateActivity : IActivity
    {
        /// <summary>
        /// Array of address added
        /// </summary>
        IList<ChannelAccount> MembersAdded { get; set; }

        /// <summary>
        /// Array of addresses removed
        /// </summary>
        IList<ChannelAccount> MembersRemoved { get; set; }

        /// <summary>
        /// Conversations new topic name
        /// </summary>
        string TopicName { get; set; }
    }
}
