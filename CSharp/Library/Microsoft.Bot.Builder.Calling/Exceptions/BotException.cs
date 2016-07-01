using System;
using System.Text;

namespace Microsoft.Bot.Builder.Calling.Exceptions
{
    /// <summary>
    ///     base exceptions for all exceptions thrown by the bots core library
    /// </summary>
    public class BotException : Exception
    {
        #region Constructors and Destructors

        /// <summary>
        ///     default constructor
        /// </summary>
        public BotException()
        {
        }

        /// <summary>
        ///     creates a new BotException
        /// </summary>
        /// <param name="message">exception message</param>
        public BotException(string message) : base(message)
        {
        }

        /// <summary>
        ///     wraps an exception into the BotException
        /// </summary>
        /// <param name="message">exception message</param>
        /// <param name="innerException">wrapped exception</param>
        /// <param name="extendForInternalExceptionRemark">
        ///     if true, message is extended with internal exception message and remark
        ///     to check it
        /// </param>
        public BotException(string message, Exception innerException, bool extendForInternalExceptionRemark = true)
            : base(
                extendForInternalExceptionRemark ? ExtendMessageWithInternalExceptionDetails(message, innerException) : message,
                innerException)
        {
        }

        #endregion

        #region Methods

        private static string ExtendMessageWithInternalExceptionDetails(string message, Exception innerException)
        {
            StringBuilder builder = new StringBuilder();
            if (message != null) builder.Append(message);
            if (innerException != null)
            {
                if (message != null) builder.Append(", error: ");
                builder.Append(innerException.Message);
                builder.Append(" (see inner exception for details)");
            }

            return builder.ToString();
        }

        #endregion
    }
}