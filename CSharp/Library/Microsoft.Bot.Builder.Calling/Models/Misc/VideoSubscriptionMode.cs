using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// List of various video modes 
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<VideoSubscriptionMode>))]
    public enum VideoSubscriptionMode
    {
        /// <summary>
        /// Unknown not recognized.
        /// </summary>
        Unknown,

        /// <summary>
        /// Manual selection of user video to subscribe
        /// </summary>
        Manual,

        /// <summary>
        /// Automatically switches based on dominant speaker
        /// </summary>
        Auto
    }
}
