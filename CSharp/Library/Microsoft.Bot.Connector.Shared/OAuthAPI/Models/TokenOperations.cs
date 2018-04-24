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
    }
}
