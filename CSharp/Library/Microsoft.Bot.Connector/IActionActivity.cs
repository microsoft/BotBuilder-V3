using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// User took action on a message (button click)
    /// </summary>
    public interface IActionActivity : IActivity
    {
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
}
