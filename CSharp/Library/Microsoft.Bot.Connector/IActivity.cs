using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// basic shared properties for all activities
    /// </summary>
    public interface IActivity
    {
        /// <summary>
        /// Activity type
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Id for the activity
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// (PROPOSED) ServiceUrl
        /// </summary>
        string ServiceUrl { get; set; }

        /// <summary>
        /// Time when message was sent
        /// </summary>
        DateTime? Timestamp { get; set; }

        /// <summary>
        /// Channel this activity is associated with
        /// </summary>
        string ChannelId { get; set; }

        /// <summary>
        /// Sender address data 
        /// </summary>
        ChannelAccount From { get; set; }

        /// <summary>
        /// Conversation Address 
        /// </summary>
        ConversationAccount Conversation { get; set; }

        /// <summary>
        /// Bot's address 
        /// </summary>
        ChannelAccount Recipient { get; set; }
    }



    /// <summary>
    /// The From address is typing
    /// </summary>
    public interface ITypingActivity : IActivity
    {
    }

}
