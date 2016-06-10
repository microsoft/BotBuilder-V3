using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Someone has updated their contact list
    /// </summary>
    public interface IContactRelationUpdateActivity : IActivity
    {

        /// <summary>
        /// Add|remove
        /// </summary>
        string Action { get; set; }
    }
}
