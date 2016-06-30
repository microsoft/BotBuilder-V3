using System;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    public static class MaxValues
    {
        /// <summary>
        /// Minimum max recording duration
        /// </summary>
        public static readonly TimeSpan RecordingDuration = TimeSpan.FromMinutes(10);

        /// <summary>
        /// max number of stop tones allowed
        /// </summary>
        public static readonly uint NumberOfStopTones = 5;

        ///<summary>
        /// Maximum allowed silence once the user has started speaking before we conclude 
        /// the user is done recording.
        /// </summary>
        public static readonly TimeSpan SilenceTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Maximum initial silence allowed from the time we start the operation 
        /// before we timeout and fail the operation. 
        /// </summary>
        public static readonly TimeSpan InitialSilenceTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Mamimum allowed time between digits if we are doing dtmf based choice recognition or CollectDigits recognition
        /// </summary>
        public static readonly TimeSpan InterDigitTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Maximum number of digits expected
        /// </summary>
        public static readonly uint NumberOfDtmfsExpected = 20;

        /// <summary>
        /// Maximum number of speech variations for a choice
        /// </summary>
        public static readonly uint NumberOfSpeechVariations = 5;

        /// <summary>
        /// max silent prompt duration
        /// </summary>
        public static readonly TimeSpan SilentPromptDuration = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Maximum number of speech variations for a choice
        /// </summary>
        public static readonly uint LengthOfTTSText = 2 * 1024;

        /// <summary>
        /// Maximum length of ApplicationState specified
        /// </summary>
        public static readonly uint AppStateLength = 1024;

        /// <summary>
        /// Max size of played prompt file.
        /// </summary>
        public static readonly uint MaxDownloadedFileSizeBytes = 1 * 1024 * 1024;

        /// <summary>
        /// Max size of MediaConfiguration in AnswerAppHostedMedia actions.
        /// </summary>
        public static readonly uint MediaConfigurationLength = 1024;

        /// <summary>
        /// Timeout downloading media file when constructing speech prompts.
        /// </summary>
        public static readonly TimeSpan FileDownloadTimeout = TimeSpan.FromSeconds(10.0);
    }
}