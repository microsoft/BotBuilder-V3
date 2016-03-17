using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A session manages the stack of dialogs used during a conversation between a bot and a user.
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// The current incoming message from the user.
        /// </summary>
        Message Message { get; }

        /// <summary>
        /// The data stored in the session, scoped to the user, conversation, or user in conversation.
        /// </summary>
        ISessionData SessionData { get; }

        /// <summary>
        /// The dialog stack managed by the session.
        /// </summary>
        IDialogFrame Stack { get; }

        /// <summary>
        /// The top-level method to respond to a message from the user.
        /// </summary>
        /// <returns>The message to return to the user.</returns>
        Task<Connector.Message> DispatchAsync();

        /// <summary>
        /// Start a child dialog.
        /// </summary>
        /// <param name="dialog">The new child dialog.</param>
        /// <param name="taskArguments">The arguments for the new child dialog.</param>
        /// <returns>The message to return to the user.</returns>
        Task<Connector.Message> BeginDialogAsync(IDialog dialog, Task<object> taskArguments);

        /// <summary>
        /// End a child dialog.
        /// </summary>
        /// <param name="dialog">The child dialog.</param>
        /// <param name="taskResult">The result from the child dialog.</param>
        /// <returns>The message to return to the user.</returns>
        Task<Connector.Message> EndDialogAsync(IDialog dialog, Task<object> taskResult);

        /// <summary>
        /// Create a message to respond to the user based on the incoming message from the user.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <returns>The message to return to the user.</returns>
        Task<Connector.Message> CreateDialogResponse(string message);

        /// <summary>
        /// Create a message to respond to the user based on the incoming message from the user.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The message to return to the user.</returns>
        Task<Connector.Message> CreateDialogResponse(Message message);
    }

    public static partial class Extensions
    {
        /// <summary>
        /// End a child dialog.
        /// </summary>
        /// <param name="session">The session that manages this dialog.</param>
        /// <param name="dialog">The child dialog.</param>
        /// <param name="result">The result from the child dialog.</param>
        /// <returns>The message to return to the user.</returns>
        public static Task<Connector.Message> EndDialogAsync(this ISession session, IDialog dialog, object result)
        {
            if (result is Task)
            {
                throw new ArgumentException(nameof(result));
            }

            return session.EndDialogAsync(dialog, Task.FromResult(result));
        }
    }
}
