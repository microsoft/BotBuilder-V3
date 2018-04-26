namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Names for event operations in the token protocol
    /// </summary>
    public static class TokenOperations
    {
        /// <summary>
        /// Name for the Token Request operation event
        /// </summary>
        /// <remarks>
        /// This event operation includes a TokenRequest object and returns a TokenResponse object
        /// </remarks>
        public const string TokenRequestOperationName = "tokens/request";

        /// <summary>
        /// Name for the Token Response operation event
        /// </summary>
        /// <remarks>
        /// This event operation represents the TokenResponse object
        /// </remarks>
        public const string TokenResponseOperationName = "tokens/response";

        /// <summary>
        /// The name of an Invoke activity that MS Teams uses to send a verification code back to the bot
        /// </summary>
        public const string TeamsVerificationCode = "signin/verifyState";
    }
}
