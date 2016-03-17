using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A collection of dialogs registered for usage by the session.
    /// </summary>
    /// <remarks>
    /// Since the dialog stack must be serializable, this class allows IDialog instance to be mapped bidirectionally to dialog IDs.
    /// </remarks>
    public interface IDialogCollection
    {
        /// <summary>
        /// Add a dialog to the collection.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <returns>This dialog collection, for fluent chaining.</returns>
        IDialogCollection Add(IDialog dialog);

        /// <summary>
        /// Get a dialog from the collection by dialog ID.
        /// </summary>
        /// <param name="ID">The dialog ID.</param>
        /// <returns>The dialog.</returns>
        IDialog Get(string ID);
    }

    public class DialogCollection : IDialogCollection
    {
        private Dictionary<string, IDialog> dialogByID;

        public DialogCollection(params IDialog[] dialogs)
            : this((IEnumerable<IDialog>) dialogs)
        {
        }

        public DialogCollection(IEnumerable<IDialog> dialogs)
        {
            this.dialogByID = new Dictionary<string, IDialog>()
            {
                { PromptDialog.Instance.ID, PromptDialog.Instance }
            };

            foreach (var dialog in dialogs)
            {
                this.Add(dialog);
            }
        }

        public IDialogCollection Add(IDialog dialog)
        {
            this.dialogByID.Add(dialog.ID, dialog);
            return this;
        }

        public IDialog Get(string ID)
        {
            IDialog dialog;
            if (!dialogByID.TryGetValue(ID, out dialog))
            {
                dialog = null;
            }

            return dialog;
        }
    }
}
