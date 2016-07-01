using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// Specialized StringEnumConverter that returns the default enum value instead of throwing if the string
    /// cannot be matched to an enum value during deserialization.
    /// </summary>
    public class StringEnumConverterWithDefault<TEnum> : StringEnumConverter where TEnum : struct
    {
        public StringEnumConverterWithDefault()
        {
            this.CamelCaseText = true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string s = reader.Value.ToString();
            TEnum t;
            Enum.TryParse<TEnum>(s, ignoreCase: true, result: out t);
            return t;
        }
    }
}
