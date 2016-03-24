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
    public interface IDialog<in T>
    {
        Task StartAsync(IDialogContext context, IAwaitable<T> arguments);
    }
}
