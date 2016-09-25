using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public interface IActivitySource
    {
        /// <summary>
        /// Produce an enumeration over conversation in time reversed order.
        /// </summary>
        /// <param name="channelId">Channel where conversation happened.</param>
        /// <param name="conversationId">Conversation within the channel.</param>
        /// <param name="max">Maximum number of activities to return.</param>
        /// <param name="oldest">Don't include any activity older than this time span.</param>
        /// <returns>Enumeration over the recorded activities.</returns>
        IEnumerable<IActivity> Activities(string channelId, string conversationId, int? max = null, TimeSpan oldest = default(TimeSpan));
    }
}
