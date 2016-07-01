using Newtonsoft.Json;
namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// The various possible states of a AudioVideoCall.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<CallState>))]
    public enum CallState
    {
        /// <summary>
        /// Unknown not recognized.
        /// </summary>
        Unknown,

        /// <summary>
        /// Initial state
        /// </summary>
        Idle,

        /// <summary>
        /// The call has just been received
        /// </summary>
        Incoming,

        /// <summary>
        /// The call establishment is in progress after initiating or accepting the call
        /// </summary>
        Establishing,

        /// <summary>
        /// The call is established
        /// </summary>
        Established,

        /// <summary>
        /// The call is on Hold
        /// </summary>
        Hold,

        /// <summary>
        /// The call is Unhold
        /// </summary>
        Unhold,

        /// <summary>
        /// The call has initiated a transfer
        /// </summary>
        Transferring,

        /// <summary>
        /// The call has initiated a redirection
        /// </summary>
        Redirecting,

        /// <summary>
        /// The call is terminating
        /// </summary>
        Terminating,

        /// <summary>
        /// The call has terminated
        /// </summary>
        Terminated,
    }
}