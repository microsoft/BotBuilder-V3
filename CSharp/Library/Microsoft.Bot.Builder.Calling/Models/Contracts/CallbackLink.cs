using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    [JsonObject(MemberSerialization.OptOut)]
    public class CallbackLink
    {
        [JsonProperty(Required = Required.Always)]
        public Uri Callback { get; set; }
    }
}
