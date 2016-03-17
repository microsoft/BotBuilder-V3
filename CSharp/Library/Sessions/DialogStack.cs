using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    internal sealed class DialogStack : IDialogStack
    {
        [Serializable]
        private class DialogFrame
        {
            public string ID { set; get; }

            public Dictionary<string, object> State { set; get; }
        }

        private readonly ISessionData sessionData;
        private readonly IDialogCollection dialogs;

        private readonly Stack<DialogFrame> stack;

        private const string FieldName = "ADA88F3B7BA5_DIALOG_STACK";

        public DialogStack(ISessionData sessionData, IDialogCollection dialogs)
        {
            Field.SetNotNull(ref this.sessionData, nameof(sessionData), sessionData);
            Field.SetNotNull(ref this.dialogs, nameof(dialogs), dialogs);

            object encoded = sessionData.GetPerUserInConversationData(FieldName);

            if (encoded != null)
            {
                this.stack = Serializers.BinarySerializer.Deserialize<Stack<DialogFrame>>((string)encoded);
            }

            if (this.stack == null)
            {
                this.stack = new Stack<DialogFrame>();
            }
        }

        void IDialogStack.Flush()
        {
            var encoded = Serializers.BinarySerializer.Serialize(this.stack);
            this.sessionData.SetPerUserInConversationData(FieldName, encoded);
        }

        int IDialogStack.Count { get { return this.stack.Count; } }

        void IDialogStack.Clear()
        {
            this.stack.Clear();
        }

        void IDialogStack.Push(IDialog dialog)
        {
            var existing = this.dialogs.Get(dialog.ID);
            if (existing == null)
            {
                throw new InvalidSessionException($"could not find dialog with ID={dialog.ID}");
            }

            if (!object.ReferenceEquals(dialog, this.dialogs.Get(dialog.ID)))
            {
                throw new ArgumentOutOfRangeException(nameof(dialog));
            }

            this.stack.Push(new DialogFrame() { ID = dialog.ID });
        }

        IDialog IDialogStack.Pop()
        {
            return this.dialogs.Get(this.stack.Pop().ID);
        }

        IDialog IDialogStack.Peek()
        {
            return this.dialogs.Get(this.stack.Peek().ID);
        }

        IDialog IDialogFrame.Dialog
        {
            get
            {
                return this.dialogs.Get(this.stack.Peek().ID);
            }
        }

        object IDialogFrame.GetLocal(string key)
        {
            var top = this.stack.Peek();
            var state = top.State;
            if (state != null)
            {
                object value;
                if (state.TryGetValue(key, out value))
                {
                    return value;
                }
            }

            return null;
        }

        void IDialogFrame.SetLocal(string key, object value)
        {
            var top = this.stack.Peek();
            if (top.State == null)
            {
                top.State = new Dictionary<string, object>();
            }

            top.State[key] = value;
        }
    }
}
