using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Dialog state managed by the dialog stack within the session.
    /// </summary>
    public interface IDialogFrame
    {
        /// <summary>
        /// The dialog for this frame.
        /// </summary>
        IDialog Dialog { get; }

        /// <summary>
        /// Set the local dialog state for this frame.
        /// </summary>
        /// <param name="key">The state key.</param>
        /// <param name="value">The state value.  This value should be serializable.</param>
        void SetLocal(string key, object value);

        /// <summary>
        /// Gets the local dialog state for this frame.
        /// </summary>
        /// <param name="key">The state key.</param>
        /// <returns>The state value.</returns>
        object GetLocal(string key);
    }

    /// <summary>
    /// Dialog stack managed by the session.
    /// </summary>
    public interface IDialogStack : IDialogFrame
    {
        /// <summary>
        /// The count of dialog frames in the stack.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Clear the stack.
        /// </summary>
        void Clear();

        /// <summary>
        /// Push a new dialog frame on to the stack.
        /// </summary>
        /// <param name="dialog">The new dialog.</param>
        void Push(IDialog dialog);

        /// <summary>
        /// Pop a dialog frame from the stack.
        /// </summary>
        /// <returns>The popped dialog.</returns>
        IDialog Pop();

        /// <summary>
        /// Peek at the top dialog frame from the stack.
        /// </summary>
        /// <returns>The top dialog.</returns>
        IDialog Peek();

        /// <summary>
        /// Flush the stack to its backing storage.
        /// </summary>
        void Flush();
    }
}
