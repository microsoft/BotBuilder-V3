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
    // <summary>
    /// A dialog is a suspendable conversational process.
    /// </summary>
    /// <remarks>
    /// Dialogs can call child dialogs or send messages to a user.
    /// Dialogs are suspended when waiting for a message from the user to the bot.
    /// Dialogs are resumed when the bot receives a message from the user.
    /// </remarks>
    /// <typeparam name="T">The start arguments.</typeparam>
    public interface IDialog<in T>
    {
        /// <summary>
        /// The start of the code that represents the conversational dialog.
        /// </summary>
        /// <param name="context">The dialog context.</param>
        /// <param name="arguments">Dialog start arguments.</param>
        /// <returns>A task that represents the dialog start.</returns>
        Task StartAsync(IDialogContext context, IAwaitable<T> arguments);
    }
}
