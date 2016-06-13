using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace Microsoft.Bot.Connector
{
    public partial class BotData
    {
        /// <summary>
        /// Get a property from a BotData recorded retrieved using the REST API
        /// </summary>
        /// <param name="property">property name to change</param>
        /// <returns>property requested or default for type</returns>
        public TypeT GetProperty<TypeT>( string property)
        {
            if (this.Data == null)
                return default(TypeT);
            return GetPropertyData<TypeT>(this.Data, property);
        }


        /// <summary>
        /// Set a property on a BotData record retrieved using the REST API
        /// </summary>
        /// <param name="property">property name to change</param>
        /// <param name="data">new data</param>
        public void SetProperty<TypeT>( string property, TypeT data)
        {
            if (this.Data == null)
                this.Data = new JObject();
            SetPropertyData(this.Data, property, data);
        }

        private TypeT GetPropertyData<TypeT>(dynamic dynamicData, string property)
        {
            if (dynamicData?[property] == null)
                return default(TypeT);
            else if (typeof(TypeT) == typeof(byte[]))
                return (TypeT)(dynamic)Convert.FromBase64String((string)dynamicData?[property]);
            else if (typeof(TypeT).IsValueType)
                return (TypeT)dynamicData?[property];
            return dynamicData?[property]?.ToObject<TypeT>();
        }

        private dynamic SetPropertyData(dynamic dynamicData, string property, object data)
        {
            if (data == null)
                dynamicData.Remove(property);
            else if (data is byte[])
                dynamicData[property] = Convert.ToBase64String((byte[])data);
            else if (data is string)
                dynamicData[property] = (string)data;
            else if (data.GetType().IsValueType)
                dynamicData[property] = JValue.FromObject(data);
            else if (data.GetType().IsArray)
                dynamicData[property] = JArray.FromObject(data);
            else
                dynamicData[property] = JObject.FromObject(data);
            return dynamicData;
        }

    }
}
