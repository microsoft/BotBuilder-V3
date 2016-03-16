using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IDialogCollection
    {
        IDialogCollection Add(IDialog dialog);
        IDialog Get(string Id);
    }

    public class DialogCollection : IDialogCollection
    {
        private Dictionary<string, IDialog> dialogs;

        public DialogCollection(params IDialog[] dialogs)
            : this((IEnumerable<IDialog>) dialogs)
        {
        }

        public DialogCollection(IEnumerable<IDialog> dialogs)
        {
            this.dialogs = new Dictionary<string, IDialog>()
            {
                { PromptDialog.Instance.Id, PromptDialog.Instance }
            };

            foreach (var dialog in dialogs)
            {
                this.Add(dialog);
            }
        }

        public IDialogCollection Add(IDialog dialog)
        {
            this.dialogs.Add(dialog.Id, dialog);
            return this;
        }

        public IDialog Get(string Id)
        {
            IDialog dialog;
            if (!dialogs.TryGetValue(Id, out dialog))
            {
                dialog = null;
            }

            return dialog;
        }
    }
}
