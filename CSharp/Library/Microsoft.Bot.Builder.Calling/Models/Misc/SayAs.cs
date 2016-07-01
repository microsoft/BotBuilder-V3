using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// Difference SayAs attributes we support for tts
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<SayAs>))]
    public enum SayAs
    {
        /// <summary>
        /// Unknown not recognized.
        /// </summary>
        Unknown,

        YearMonthDay,

        MonthDayYear,

        DayMonthYear,

        YearMonth,

        MonthYear,

        MonthDay,

        DayMonth,

        Day,

        Month,

        Year,

        Cardinal,

        Ordinal,

        Letters,

        Time12,

        Time24,

        Telephone,

        Name,

        PhoneticName,
    }
}