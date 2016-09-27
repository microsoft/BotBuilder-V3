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
    public interface ITriggerActivity : IActivity
    {
        /// <summary>
        /// Value from the trigger source
        /// </summary>
        object Value { get; set; }
    }
}
