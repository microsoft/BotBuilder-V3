using System;
using System.Linq;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Someone has updated their contact list
    /// </summary>
    public interface IContactRelationUpdateActivity : IActivity
    {

        /// <summary>
        /// add|remove
        /// </summary>
        string Action { get; set; }
    }
}
