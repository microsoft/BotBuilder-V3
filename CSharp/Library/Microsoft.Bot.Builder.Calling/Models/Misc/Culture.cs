using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// Cultures we support for recognition or prompt playing
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<Culture>))]
    public enum Culture
    {
        /// <summary>
        /// Unknown not recognized.
        /// </summary>
        Unknown,

        [EnumMember(Value = "en-US")]
        EnUs,
    }
}