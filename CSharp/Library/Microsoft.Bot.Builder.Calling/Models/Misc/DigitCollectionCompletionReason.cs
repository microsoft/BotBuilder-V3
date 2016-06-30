using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// Reason for completion of Digit Collection Operation
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<DigitCollectionCompletionReason>))]
    public enum DigitCollectionCompletionReason
    {
        /// <summary>
        /// Unknown not recognized.
        /// </summary>
        Unknown,

        /// <summary>
        /// The max time period by which user is supposed to start punching in digits has elapsed.
        /// 
        /// This results in a "failed" DigitCollection Attempt
        /// </summary>
        InitialSilenceTimeout,

        /// <summary>
        /// The maximum time period between user punching in successive digits has elapsed.
        /// 
        /// This results in a "successful" DigitCollection Attempt and we return the digits collected till then.
        /// </summary>
        InterdigitTimeout,

        /// <summary>
        /// Digit collection attempt was stopped by user punching in a stop tone.
        /// 
        /// This results in a "successful" DigitCollection Attempt and we return the digits collected till then. 
        /// The stopTone(s) detected are excluded from the digits we return.
        /// </summary>
        CompletedStopToneDetected,

        /// <summary>
        /// The underlying call was terminated
        /// 
        /// This results in a "failed" DigitCollection Attempt
        /// </summary>
        CallTerminated,

        /// <summary>
        /// Misc System Failure
        /// </summary>
        TemporarySystemFailure,
    }
}