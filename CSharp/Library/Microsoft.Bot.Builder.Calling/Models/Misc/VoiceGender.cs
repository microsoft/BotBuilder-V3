using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// Voice genders we support for tts
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<VoiceGender>))]
    public enum VoiceGender
    {
        Male,

        Female,
    }
}