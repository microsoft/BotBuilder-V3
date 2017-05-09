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

namespace Microsoft.Bot.Builder.Calling.Exceptions
{
    /// <summary>
    ///     Exception type thrown when the bot configuration is invalid
    /// </summary>
    public class BotConfigurationException : BotException
    {
        #region Public Properties

        /// <summary>
        ///     identifier of the configuration item which caused the failure (may stay null in case of global failures)
        /// </summary>
        public string ConfigurationItemName { get; private set; }

        /// <summary>
        ///     value of the configuration item which caused the exception
        /// </summary>
        public string ConfigurationItemValue { get; private set; }

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     creates a new BotConfigurationException
        /// </summary>
        /// <param name="message">exception message</param>
        /// <param name="itemName">relevant configuration item name</param>
        /// <param name="itemValue">relevant configuration item value</param>
        public BotConfigurationException(string message, string itemName = null, string itemValue = null) : base(message)
        {
            ConfigurationItemName = itemName;
            ConfigurationItemValue = itemValue;
        }

        /// <summary>
        ///     wraps an exception into the BotConfigurationException
        /// </summary>
        /// <param name="message">exception message</param>
        /// <param name="innerException">wrapped exception</param>
        /// <param name="itemName">relevant configuration item name</param>
        /// <param name="itemValue">relevant configuration item value</param>
        /// <param name="extendForInternalExceptionRemark">
        ///     if true, message is extended with internal exception message and remark
        ///     to check it
        /// </param>
        public BotConfigurationException(
            string message,
            Exception innerException,
            string itemName = null,
            string itemValue = null,
            bool extendForInternalExceptionRemark = true) : base(message, innerException, extendForInternalExceptionRemark)
        {
            ConfigurationItemName = itemName;
            ConfigurationItemValue = itemValue;
        }

        #endregion
    }
}