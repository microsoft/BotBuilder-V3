using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// Reason for completion of Recording Operation
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<RecordingCompletionReason>))]
    public enum RecordingCompletionReason
    {
        /// <summary>
        /// Unknown not recognized.
        /// </summary>
        Unknown,

        /// <summary>
        /// The maximum initial silence that can be tolerated had been reached
        /// 
        /// This results in a "failed" Recording Attempt
        /// </summary>
        InitialSilenceTimeout,

        /// <summary>
        /// The maximum duration that can be allowed for recording had been reached
        /// 
        /// This results in a "successful" Recording Attempt
        /// </summary>
        MaxRecordingTimeout,

        /// <summary>
        /// Recording was completed as detected by silence after a talk spurt
        /// 
        /// This results in a "successful" Recording Attempt
        /// </summary>
        CompletedSilenceDetected,

        /// <summary>
        /// Recording was completed by user punching in a stop tone
        /// 
        /// This results in a "successful" Recording Attempt
        /// </summary>
        CompletedStopToneDetected,

        /// <summary>
        /// The underlying call was terminated
        /// 
        /// This results in a "successful" Recording Attempt if there were any bytes recorded
        /// </summary>
        CallTerminated,

        /// <summary>
        /// Misc System Failure
        /// </summary>
        TemporarySystemFailure,
    }
}