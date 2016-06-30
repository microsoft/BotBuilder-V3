using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// Encoding format to be used for recording
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<RecordingFormat>))]
    public enum RecordingFormat
    {
        /// <summary>
        /// Unknown not recognized.
        /// </summary>
        Unknown,

        /// <summary>
        /// WMA
        /// </summary>
        Wma,

        /// <summary>
        /// WAV
        /// </summary>
        Wav,

        /// <summary>
        /// MP3
        /// </summary>
        Mp3,
    }
}