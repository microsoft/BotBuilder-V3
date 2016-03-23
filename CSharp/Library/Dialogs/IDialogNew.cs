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
    public delegate Task ResumeAfter<in T>(IDialogContext context, IAwaitable<T> result);

    public interface IDialogStackNew
    {
        void Wait(ResumeAfter<Message> resume);
        void Call<T, R>(T child, ResumeAfter<R> resume) where T : class, IDialogNew;
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


    public interface IDialogContext : IBotData, IDialogStackNew, IBotToUser
    {
        Task PostAsync(string text, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IDialogNew
    {
        Task StartAsync(IDialogContext context, IAwaitable<object> arguments);
    }
}
