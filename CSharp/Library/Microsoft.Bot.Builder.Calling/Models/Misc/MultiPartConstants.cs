namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    public static class MultiPartConstants
    {
        /// <summary>
        /// content disposition name to use for the binary recorded audio (in a multipart response)
        /// </summary>
        public static readonly string RecordingContentDispositionName = "recordedAudio";

        /// <summary>
        /// content disposition name to use for the result object (in a multipart response)
        /// </summary>
        public static readonly string ResultContentDispositionName = "conversationResult";

        /// <summary>
        /// mime type for wav
        /// </summary>
        public static readonly string WavMimeType = "audio/wav";

        /// <summary>
        /// mime type for wma
        /// </summary>
        public static readonly string WmaMimeType = "audio/x-ms-wma";

        /// <summary>
        /// mime type for mp3
        /// </summary>
        public static readonly string Mp3MimeType = "audio/mpeg";
    }
}