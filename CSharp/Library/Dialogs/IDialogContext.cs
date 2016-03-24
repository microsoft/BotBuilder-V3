using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public delegate Task ResumeAfter<in T>(IDialogContext context, IAwaitable<T> result);

    public interface IDialogStack
    {
        void Wait(ResumeAfter<Message> resume);
        void Call<C, T, R>(C child, T arguments, ResumeAfter<R> resume) where C : class, IDialog<T>;
        void Done<R>(R value);
    }

    public interface IBotToUser
    {
        Task PostAsync(Message message, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IUserToBot
    {
        Task<Message> PostAsync(Message message, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IDialogContext : IBotData, IDialogStack, IBotToUser
    {
        Task PostAsync(string text, CancellationToken cancellationToken = default(CancellationToken));
    }

    public static partial class Extensions
    {
        public static void Call<C, T, R>(this IDialogContext context, C child, ResumeAfter<R> resume) where C : class, IDialog<T>
        {
            context.Call<C, T, R>(child, default(T), resume);
        }

        public static void Call<C, T, R>(this IDialogContext context, ResumeAfter<R> resume) where C : class, IDialog<T>, new()
        {
            context.Call<C, T, R>(new C(), default(T), resume);
        }
    }
}
