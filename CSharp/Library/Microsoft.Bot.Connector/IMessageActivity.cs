using System;
using System.Collections.Generic;
using System.Linq;

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
        /// True if this activity has text, attachments, or channelData
        /// </summary>
        bool HasContent();

        /// <summary>
        /// Get channeldata as typed structure
        /// </summary>
        /// <typeparam name="TypeT">type to use</typeparam>
        /// <returns>typed object or default(TypeT)</returns>
        TypeT GetChannelData<TypeT>();

        /// <summary>
        /// Get mentions
        /// </summary>
        Mention[] GetMentions();
    }
}
