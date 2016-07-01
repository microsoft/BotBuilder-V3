using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// Reason for completion of Recognition(speech/dtmf) Operation
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<RecognitionCompletionReason>))]
    public enum RecognitionCompletionReason
    {
        /// <summary>
        /// Unknown not recognized.
        /// </summary>
        Unknown,

        /// <summary>
        /// The maximum initial silence that can be tolerated had been reached
        /// 
        /// This results in a "failed" Recognition Attempt
        /// </summary>
        InitialSilenceTimeout,

        /// <summary>
        /// The Recognition completed because the user punched in wrong dtmf which was not amongst the possible 
        /// choices. 
        /// 
        /// We would only look for dtmfs when dtmf recognition is requested. Thus for pure speech menus, this
        /// completion reason would never be possible.
        /// 
        /// This results in a "failed" Recognition Attempt
        /// </summary>
        IncorrectDtmf,

        /// <summary>
        /// The maximum time period between user punching in successive digits has elapsed.
        /// 
        /// We would only look for dtmfs when dtmf recognition is requested. Thus for pure speech menus, this
        /// completion reason would never be possible.
        /// 
        /// This results in a "failed" Recognition Attempt.
        /// </summary>
        InterdigitTimeout,

        /// <summary>
        /// The recognition successfully matched a Grammar option
        /// </summary>
        SpeechOptionMatched,

        /// <summary>
        /// The recognition successfully matched a Dtmf option
        /// </summary>
        DtmfOptionMatched,

        /// <summary>
        /// The underlying call was terminated
        /// 
        /// This results in a "failed" Recognition Attempt
        /// </summary>
        CallTerminated,

        /// <summary>
        /// Misc System Failure
        /// </summary>
        TemporarySystemFailure,
    }
}