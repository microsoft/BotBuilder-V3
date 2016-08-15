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

        /// <summary>
        /// Channel specific payload
        /// </summary>
        /// <remarks>
        /// Some channels will provide channel specific data.
        /// 
        /// For a message originating in the channel it might provide the original native schema object for the channel. 
        /// 
        /// For a message coming into the channel it might accept a payload allowing you to create a "native" response for the channel.
        /// 
        /// Example:
        /// * Email - The Email Channel will put the original Email metadata into the ChannelData object for outgoing messages, and will accep
        /// on incoming message a Subject property, and a HtmlBody which can contain Html.  
        /// 
        /// The channel data essentially allows a bot to have access to native functionality on a per channel basis.
        /// </remarks>
        dynamic ChannelData { get; set; }
    }



    /// <summary>
    /// The From address is typing
    /// </summary>
    public interface ITypingActivity : IActivity
    {
    }

}
