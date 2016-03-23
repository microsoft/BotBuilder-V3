using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IAwaiter<out T> : INotifyCompletion
    {
        bool IsCompleted { get; }

        T GetResult();
    }

    public interface IAwaitable<out T>
    {
        IAwaiter<T> GetAwaiter();
    }
}
