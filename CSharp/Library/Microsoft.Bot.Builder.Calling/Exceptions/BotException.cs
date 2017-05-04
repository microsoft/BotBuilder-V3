// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
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