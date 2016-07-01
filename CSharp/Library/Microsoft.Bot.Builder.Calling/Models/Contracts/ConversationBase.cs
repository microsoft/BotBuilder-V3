using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This base class defines a subset of properties which define a conversation.
    /// Conversation class derives from this and adds more properties - they are passed in OnIncomingCall
    /// ConversationResultBase class derives from this and adds more properties - they are passed in POST to callback Url to list operation outcomes
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public abstract class ConversationBase
    {
        /// <summary>
        /// Conversation Id 
        /// </summary>
        [JsonProperty(Required = Required.Always, Order = -2)]
        public string Id { get; set; }

        /// <summary>
        /// AppId of the customer ( if any )
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string AppId { get; set; }

        /// <summary>
        /// Opaque string to facilitate app developers to pass their custom data in this field. 
        /// This field is the same value that was passed 'response' by the customer.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string AppState { get; set; }

        /// <summary>
        /// Any links we want to surface to the customer for them to invoke us back on.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public Dictionary<string, Uri> Links { get; set; }

        public virtual void Validate()
        {
            Utils.AssertArgument(!String.IsNullOrWhiteSpace(this.Id), "Id cannot be null or empty");
            ApplicationState.Validate(this.AppState);
        }
    }
}
