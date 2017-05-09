using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This defines the set of the properties that define a conversation. 
    /// A conversation includes participants, modalities etc.
    /// 
    /// This object is specified in the body of the OnIncomingCall request sent to the client.
    /// This object is used to represent both incoming and outgoing conversations.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Conversation : ConversationBase
    {
        /// <summary>
        /// List of participants in the conversation
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public IEnumerable<Participant> Participants { get; set; }

        /// <summary>
        /// Indicates whether a call is a group call
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public bool IsMultiparty { get; set; }

        /// <summary>
        /// Identifies a specfic topic within a chat thread.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string ThreadMessageId { get; set; }

        /// <summary>
        /// Id for the chat thread
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string ThreadId { get; set; }

        /// <summary>
        /// Different modalities which are presented in the call
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public IEnumerable<ModalityType> PresentedModalityTypes { get; set; }

        /// <summary>
        /// Current state of the Call
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public CallState CallState { get; set; }

        /// <summary>
        /// Subject of the call 
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string Subject { get; set; }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        [JsonIgnore]
        public IDictionary<string, JToken> AdditionalData
        {
            get
            {
                if (_additionalData == null)
                {
                    _additionalData = new Dictionary<string, JToken>();
                }

                return _additionalData;
            }
            set
            {
                _additionalData = value;
            }
        }

        public override void Validate()
        {
            base.Validate();
            Participant.Validate(this.Participants);
            if (IsMultiparty)
            {
                Utils.AssertArgument(!String.IsNullOrWhiteSpace(ThreadId), "ThreadId has to be specified for a multiparty call.");
            }
            Utils.AssertArgument(this.PresentedModalityTypes != null && this.PresentedModalityTypes.Any(), "Call must be presented with atleast one modality");
        }
    }
}
