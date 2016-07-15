using System.Collections.Generic;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This class contains the response the customer sent for the notification POST to their callback url.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class NotificationResponse
    {
        /// <summary>
        /// Callback link to call back the customer on, once we have processed the notification response from customer.
        /// 
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public CallbackLink Links { get; set; }

        /// <summary>
        /// Opaque string to facilitate app developers to pass their custom data in this field. 
        /// This field is echo'd back in the 'result' POST for this 'response'.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string AppState { get; set; }

        public virtual void Validate()
        {
            if (this.Links != null)
            {
                Utils.AssertArgument(this.Links.Callback != null, "Callback link cannot be specified as null");
                Utils.AssertArgument(this.Links.Callback.IsAbsoluteUri, "Callback link must be an absolute uri");
            }
            ApplicationState.Validate(this.AppState);
        }
    }
}
