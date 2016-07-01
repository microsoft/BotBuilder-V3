using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// Possible outcomes 
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<Outcome>))]
    public enum Outcome
    {
        /// <summary>
        /// Success
        /// </summary>
        Success,

        /// <summary>
        /// Failure
        /// </summary>
        Failure,
    }
}