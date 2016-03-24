using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Form
{
    /// <summary>
    /// Initial state for a Microsoft.Bot.Builder.Form.Form.
    /// </summary>
    /// <remarks>
    /// If a parent dialog wants to pass in the initial state of the form, you would use this structure.
    /// It includes both the state and optionally initial entities from a LUIS dialog that will be used to 
    /// initially populate the form state.
    /// </remarks>
    [Serializable]
    public class InitialState<T>
    {
        /// <summary>
        /// Default form state.
        /// </summary>
        public T State { get; set; }

        /// <summary>
        /// LUIS entities to put into state.
        /// </summary>
        /// <remarks>
        /// In order to set a field in the form state, the Entity must be named with the path to the field in the form state.
        /// </remarks>
        public Models.EntityRecommendation[] Entities { get; set; }

        /// <summary>
        /// Whether this form should prompt the user when started.
        /// </summary>
        public bool PromptInStart { get; set; }
    }
}
