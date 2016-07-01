using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the action which customers can specify to indicate that the server call agent should place an outgoing call.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class PlaceCall : ActionBase
    {
        /// <summary>
        /// MRI for the source of the call
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Participant Source { get; set; }

        /// <summary>
        /// MRI of the user to whom the call is to be placed
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Participant Target { get; set; }

        /// <summary>
        /// Subject of the call that is to be placed
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string Subject { get; set; }

        public static readonly IEnumerable<ModalityType> DefaultModalityTypes = new ModalityType[] { ModalityType.Audio };

        private IEnumerable<ModalityType> initiateModalityTypes;

        /// <summary>
        /// The modality types the application want to present.  If none are specified,
        /// audio-only is assumed.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public IEnumerable<ModalityType> ModalityTypes
        {
            get
            {
                if (this.initiateModalityTypes == null || !this.initiateModalityTypes.Any())
                {
                    return DefaultModalityTypes;
                }
                else
                {
                    return this.initiateModalityTypes;
                }
            }

            set
            {
                this.initiateModalityTypes = value;
            }
        }
        /// <summary>
        /// AppId of the customer 
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string AppId { get; set; }

        public PlaceCall()
        {
            this.Action = ValidActions.PlaceCallAction;
        }

        public override void Validate()
        {
            base.Validate();

            Utils.AssertArgument(Target != null, "Target cannot be null");
            Utils.AssertArgument(Source != null, "Source cannot be null");
            Utils.AssertArgument(!String.IsNullOrWhiteSpace(AppId), "AppId cannot be null or empty");
            Utils.AssertArgument(this.ModalityTypes.Distinct().Count() == this.ModalityTypes.Count(), "ModalityTypes cannot contain duplicate elements.");
            Utils.AssertArgument(this.ModalityTypes.All((m) => { return m != ModalityType.Unknown; }), "ModalityTypes contains an unknown media type.");
            Utils.AssertArgument(this.ModalityTypes.All((m) => { return m != ModalityType.VideoBasedScreenSharing; }), "ModalityTypes cannot contain VideoBasedScreenSharing.");

            Source.Validate();
            Target.Validate();
            Source.Originator = true;
            Target.Originator = false;
        }
    }
}
