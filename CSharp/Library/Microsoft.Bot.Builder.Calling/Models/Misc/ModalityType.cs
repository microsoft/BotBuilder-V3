using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// List of supported modality types
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<ModalityType>))]
    public enum ModalityType
    {
        Unknown,

        Audio,

        Video,

        VideoBasedScreenSharing
    }
}