using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// Helper class for serializing/deserializing
    /// </summary>
    public static class Serializer
    {
        private static readonly JsonSerializerSettings defaultSerializerSettings = Serializer.GetSerializerSettings();
        private static readonly JsonSerializerSettings loggingSerializerSettings = Serializer.GetSerializerSettings(Formatting.Indented);

        /// <summary>
        /// Serialize input object to string
        /// </summary>
        public static string SerializeToJson(object obj, bool forLogging = false)
        {
            return JsonConvert.SerializeObject(obj, forLogging ? loggingSerializerSettings : defaultSerializerSettings);
        }

        /// <summary>
        /// Serialize to JToken
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static JToken SerializeToJToken(Object obj)
        {
            return JToken.FromObject(obj, JsonSerializer.Create(defaultSerializerSettings));
        }

        /// <summary>
        /// Deserialize input string to object
        /// </summary>
        public static T DeserializeFromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, defaultSerializerSettings);
        }

        /// <summary>
        /// Deserialize from JToken
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jToken"></param>
        /// <returns></returns>
        public static T DeserializeFromJToken<T>(JToken jToken)
        {
            return jToken.ToObject<T>(JsonSerializer.Create(defaultSerializerSettings));
        }

        /// <summary>
        /// Returns default serializer settings.
        /// </summary>
        public static JsonSerializerSettings GetSerializerSettings(Formatting formatting = Formatting.None)
        {
            return new JsonSerializerSettings()
            {
                Formatting = formatting,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = true }, new ActionConverter(), new OperationOutcomeConverter(), new NotificationConverter() },
            };
        }
    }
}
