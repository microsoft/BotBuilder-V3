using Microsoft.Bot.Connector;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Encapsulates a method that represents the code to execute after a result is available.
    /// </summary>
    /// <remarks>
    /// The result is often a message from the user.
    /// </remarks>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="context">The dialog context.</param>
    /// <param name="result">The result.</param>
    /// <returns>A task that represents the code that will resume after the result is available.</returns>
    public delegate Task ResumeAfter<in T>(IDialogContext context, IAwaitable<T> result);

    /// <summary>
    /// The stack of dialogs in the conversational process.
    /// </summary>
    public interface IDialogStack
    {
        /// <summary>
        /// Suspend the current dialog until the user has sent a message to the bot.
        /// </summary>
        /// <param name="resume">The method to resume when the message has been received.</param>
        void Wait(ResumeAfter<Message> resume);

        /// <summary>
        /// Call a child dialog and add it to the top of the stack.
        /// </summary>
        /// <typeparam name="C">The type of the child dialog.</typeparam>
        /// <typeparam name="T">The type of the child dialog's argument.</typeparam>
        /// <typeparam name="R">The type of result expected from the child dialog.</typeparam>
        /// <param name="child">The child dialog.</param>
        /// <param name="argument">The child dialog's argument.</param>
        /// <param name="resume">The method to resume when the child dialog has completed.</param>
        void Call<C, T, R>(C child, T argument, ResumeAfter<R> resume) where C : class, IDialog<T>;

        /// <summary>
        /// Complete the current dialog and return a result to the parent dialog.
        /// </summary>
        /// <typeparam name="R">The type of the result dialog.</typeparam>
        /// <param name="value">The value of the result.</param>
        void Done<R>(R value);
    }

    /// <summary>
    /// Methods to send a message from the bot to the user. 
    /// </summary>
    public interface IBotToUser
    {
        /// <summary>
        /// Post a message to be sent to the user.
        /// </summary>
        /// <param name="message">The message for the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the post operation.</returns>
        Task PostAsync(Message message, CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// Methods to send a message from the user to the bot.
    /// </summary>
    public interface IUserToBot
    {
        /// <summary>
        /// Post a message to be sent to the bot.
        /// </summary>
        /// <param name="message">The message for the bot.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents an inline response message to send back to the user.</returns>
        Task<Message> PostAsync(Message message, CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// The context for the execution of a dialog's conversational process.
    /// </summary>
    public interface IDialogContext : IBotData, IDialogStack, IBotToUser
    {
        /// <summary>
        /// Post a message to be sent to the bot, using previous messages to establish a conversation context.
        /// </summary>
        /// <param name="text">The message text.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the post operation.</returns>
        Task PostAsync(string text, CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// Helper methods.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Call a child dialog and add it to the top of the stack.
        /// </summary>
        /// <typeparam name="C">The type of the child dialog.</typeparam>
        /// <typeparam name="T">The type of the child dialog's argument.</typeparam>
        /// <typeparam name="R">The type of result expected from the child dialog.</typeparam>
        /// <param name="context">The dialog context.</param>
        /// <param name="child">The child dialog.</param>
        /// <param name="resume">The method to resume when the child dialog has completed.</param>
        public static void Call<C, T, R>(this IDialogContext context, C child, ResumeAfter<R> resume) where C : class, IDialog<T>
        {
            context.Call<C, T, R>(child, default(T), resume);
        }

        /// <summary>
        /// Call a child dialog and add it to the top of the stack.
        /// </summary>
        /// <typeparam name="C">The type of the child dialog.  The child dialog must have a default constructor.</typeparam>
        /// <typeparam name="T">The type of the child dialog's argument.</typeparam>
        /// <typeparam name="R">The type of result expected from the child dialog.</typeparam>
        /// <param name="context">The dialog context.</param>
        /// <param name="resume">The method to resume when the child dialog has completed.</param>
        public static void Call<C, T, R>(this IDialogContext context, ResumeAfter<R> resume) where C : class, IDialog<T>, new()
        {
            context.Call<C, T, R>(new C(), default(T), resume);
        }
    }
}
