// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Log message activities between bots and users.
    /// </summary>
    public interface IActivityLogger
    {
        Task LogAsync(IActivity activity);
    }

    /// <summary>
    /// Activity logger that traces to the console.
    /// </summary>
    /// <remarks>
    /// To use this, you need to register the class like this:
    /// <code>           
    /// var builder = new ContainerBuilder();
    /// builder.RegisterModule(Dialog_Manager.MakeRoot());
    /// builder.RegisterType&lt;TraceActivityLogger&gt;()
    ///        .AsImplementedInterfaces()
    ///        .InstancePerLifetimeScope();
    /// </code>
    /// </remarks>
    public sealed class TraceActivityLogger : IActivityLogger
    {
        /// <summary>
        /// Log activity to trace stream.
        /// </summary>
        /// <param name="activity">Activity to log.</param>
        /// <returns></returns>
        async Task IActivityLogger.LogAsync(IActivity activity)
        {
            var message = activity as IMessageActivity;
            if (message != null)
            {
                Trace.TraceInformation(message.Text);
            }
        }
    }

    /// <summary>
    /// Activity logger that doesn't log.
    /// </summary>
    public sealed class NullActivityLogger : IActivityLogger
    {
        /// <summary>
        /// Swallow activity.
        /// </summary>
        /// <param name="activity">Activity to be logged.</param>
        /// <returns></returns>
        async Task IActivityLogger.LogAsync(IActivity activity)
        {
        }
    }
}

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    public sealed class LogToBot : IPostToBot
    {
        private readonly IActivityLogger logger;
        private readonly IPostToBot inner;
        public LogToBot(IActivityLogger logger, IPostToBot inner)
        {
            SetField.NotNull(out this.logger, nameof(logger), logger);
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }

        async Task IPostToBot.PostAsync<T>(T item, CancellationToken token)
        {
            var activity = item as IActivity;
            if (activity != null)
            {
                await this.logger.LogAsync(activity);
            }
            await inner.PostAsync<T>(item, token);
        }
    }

    public sealed class LogToUser : IBotToUser
    {
        private readonly IActivityLogger logger;
        private readonly IBotToUser inner;
        public LogToUser(IActivityLogger logger, IBotToUser inner)
        {
            SetField.NotNull(out this.logger, nameof(logger), logger);
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }
        IMessageActivity IBotToUser.MakeMessage()
        {
            return this.inner.MakeMessage();
        }

        async Task IBotToUser.PostAsync(IMessageActivity message, CancellationToken cancellationToken)
        {
            await this.logger.LogAsync(message);
            await this.inner.PostAsync(message, cancellationToken);
        }
    }
}
