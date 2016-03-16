using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IDialogFrame
    {
        IDialog Dialog { get; }
        void SetDialogState<T>(string key, T data);
        object GetDialogState(string key);
    }

    public interface IDialogStack : IDialogFrame
    {
        int Count { get; }
        void Clear();
        void Push(IDialog dialog);
        IDialog Pop();
        IDialog Peek();

        void Flush();
    }
}
