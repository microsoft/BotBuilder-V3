using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the outcome of the "placecall" action. This is conveyed to the customer as POST to the customer CallBack Url.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class PlaceCallOutcome : OperationOutcomeBase
    {
        /// <summary>
        /// Different modalities which were accepted by the remote end
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public IEnumerable<ModalityType> AcceptedModalityTypes { get; set; }

        public PlaceCallOutcome()
        {
            this.Type = ValidOutcomes.PlaceCallOutcome;
        }

        public override void Validate()
        {
            base.Validate();
            if (this.Outcome == Outcome.Success)
            {
                Utils.AssertArgument(this.AcceptedModalityTypes != null && this.AcceptedModalityTypes.Any(), "Call must be accepted with atleast one modality");
            }
        }
    }
}
