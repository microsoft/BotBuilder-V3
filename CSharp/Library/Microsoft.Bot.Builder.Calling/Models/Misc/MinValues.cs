using System;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    public static class MinValues
    {
        /// <summary>
        /// JSON Content Type
        /// </summary>
        public static readonly TimeSpan RecordingDuration = TimeSpan.FromSeconds(2.0);

        ///<summary>
        /// Minimum allowed silence once the user has started speaking before we conclude 
        /// the user is done recording.
        /// </summary>
        public static readonly TimeSpan SilenceTimeout = TimeSpan.FromSeconds(0.0);

        /// <summary>
        /// Minimum initial silence allowed from the time we start the operation 
        /// before we timeout and fail the operation. 
        /// </summary>
        public static readonly TimeSpan InitialSilenceTimeout = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Mamimum allowed time between digits if we are doing dtmf based choice recognition or CollectDigits recognition
        /// </summary>
        public static readonly TimeSpan InterdigitTimeout = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Maximum number of digits expected
        /// </summary>
        public static readonly uint NumberOfDtmfsExpected = 1;
    }
}