using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IDialog
    {
        Task StartAsync(IDialogContext context, IAwaitable<object> arguments);
    }

    public delegate Task ResumeAfter<in T>(IDialogContext context, IAwaitable<T> result);

    public interface IDialogStack
    {
        void Wait(ResumeAfter<Message> resume);
        void Call<T, R>(T child, ResumeAfter<R> resume) where T : class, IDialog;
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
        public static void Call<T, R>(this IDialogContext context, ResumeAfter<R> resume) where T : class, IDialog, new()
        {
            context.Call<T, R>(new T(), resume);
        }
    }
}
