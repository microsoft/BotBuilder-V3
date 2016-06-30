using System;

namespace Microsoft.Bot.Builder.Calling.Exceptions
{
    public class BotCallingServiceException : BotException
    {
        #region Constructors and Destructors

        public BotCallingServiceException(string message)
            : base(message)
        {
        }

        public BotCallingServiceException(string message, Exception innerException, bool extendForInternalExceptionRemark = true)
            : base(message, innerException, extendForInternalExceptionRemark)
        {
        }

        #endregion
    }
}