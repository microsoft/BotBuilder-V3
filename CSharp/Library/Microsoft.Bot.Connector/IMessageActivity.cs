using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Someone has added a message to the conversation
    /// </summary>
    public interface IMessageActivity : IActivity
    {
        /// <summary>
        /// The language code of the Text field
        /// </summary>
        /// <remarks>
        /// See https://msdn.microsoft.com/en-us/library/hh456380.aspx for a list of valid language codes
        /// </remarks>
        string Locale { get; set; }

        /// <summary>
        /// Text for the message
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Text for the message
        /// </summary>
        string Summary { get; set; }

        /// <summary>
        /// Format of text fields [plain|markdown] default:markdown
        /// </summary>
        string TextFormat { get; set; }

        /// <summary>
        /// AttachmentLayout - hint for how to deal with multiple attachments Values: [list|carousel] default:list
        /// </summary>
        string AttachmentLayout { get; set; }

        /// <summary>
        /// content attachemnts
        /// </summary>
        IList<Attachment> Attachments { get; set; }

        /// <summary>
        /// Entities 
        /// Collection of objects which contain metadata about this activity
        /// </summary>
        IList<Entity> Entities { get; set; }

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


        /// <summary>
        /// the original id this message is a response to
        /// </summary>
        string ReplyToId { get; set; }

        /// <summary>
        /// Get mentions from the Entities field
        /// </summary>
        /// <returns></returns>
        Mention[] GetMentions();

        /// <summary>
        /// Get channeldata as typed structure
        /// </summary>
        /// <typeparam name="TypeT">type to use</typeparam>
        /// <returns>typed object or default(TypeT)</returns>
        TypeT GetChannelData<TypeT>();

        /// <summary>
        /// Return the "major" portion of the activity
        /// </summary>
        /// <returns>normalized major portion of the activity, aka message/... will return "message"</returns>
        string GetActivityType();

    }
}