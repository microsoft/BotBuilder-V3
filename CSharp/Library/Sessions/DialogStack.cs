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
            public string Id { set; get; }

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
            if (!object.ReferenceEquals(dialog, this.dialogs.Get(dialog.Id)))
            {
                throw new ArgumentException(nameof(dialog));
            }

            this.stack.Push(new DialogFrame() { Id = dialog.Id });
        }

        IDialog IDialogStack.Pop()
        {
            return this.dialogs.Get(this.stack.Pop().Id);
        }

        IDialog IDialogStack.Peek()
        {
            return this.dialogs.Get(this.stack.Peek().Id);
        }

        IDialog IDialogFrame.Dialog
        {
            get
            {
                return this.dialogs.Get(this.stack.Peek().Id);
            }
        }

        object IDialogFrame.GetDialogState(string key)
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

        void IDialogFrame.SetDialogState<T>(string key, T data)
        {
            var top = this.stack.Peek();
            if (top.State == null)
            {
                top.State = new Dictionary<string, object>();
            }

            top.State[key] = data;
        }
    }
}
