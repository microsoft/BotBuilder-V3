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