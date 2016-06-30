using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// List of supported video resolution formats
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<ResolutionFormat>))]
    public enum ResolutionFormat
    {
        /// <summary>
        /// Unknown not recognized.
        /// </summary>
        Unknown,

        [EnumMember(Value = "sd360p")]
        Sd360p,

        [EnumMember(Value = "sd540p")]
        Sd540p,

        [EnumMember(Value = "hd720p")]
        Hd720p,

        [EnumMember(Value = "hd1080p")]
        Hd1080p
    }
}
