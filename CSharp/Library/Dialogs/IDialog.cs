using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A dialog is an suspendable conversational process.
    /// </summary>
    /// <remarks>
    /// Dialogs can be suspended to call child dialogs or send a message to a user.
    /// Dialogs are resumed when child dialogs complete or the user sent a message to the bot.
    /// </remarks>
    public interface IDialog
    {
        /// <summary>
        /// The dialog ID
        /// </summary>
        /// <remarks>
        /// Since dialogs are immutable and all state is stored in the stack managed by the session,
        /// this ID tends to be a constant for each type of IDialog implementation.
        /// </remarks>
        string ID { get; }

        /// <summary>
        /// Initiate the dialog.
        /// </summary>
        /// <param name="session">The session that manages the dialog in the stack.</param>
        /// <param name="taskArguments">The arguments for the child dialog.</param>
        /// <returns>The message to return to the user.</returns>
        Task<Connector.Message> BeginAsync(ISession session, Task<object> taskArguments);

        /// <summary>
        /// Respond to a message sent by the user.
        /// </summary>
        /// <param name="session">The session that manages the dialog in the stack.</param>
        /// <returns>The message to return to the user.</returns>
        Task<Connector.Message> ReplyReceivedAsync(ISession session);

        /// <summary>
        /// Respond to the result from a child dialog.
        /// </summary>
        /// <param name="session">The session that manages the dialog in the stack.</param>
        /// <param name="taskResult">The result from the child dialog.</param>
        /// <returns>The message to return to the user.</returns>
        Task<Connector.Message> DialogResumedAsync(ISession session, Task<object> taskResult);
    }
}
