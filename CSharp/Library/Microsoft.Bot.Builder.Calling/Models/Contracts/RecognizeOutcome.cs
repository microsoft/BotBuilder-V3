using System;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the outcome of the "recognize" action. This is conveyed to the customer as POST to the customer CallBack Url.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class RecognizeOutcome : OperationOutcomeBase
    {
        /// <summary>
        /// Outcome of the Choice based recognition ( if any was specified in the action )
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public ChoiceOutcome ChoiceOutcome { get; set; }

        /// <summary>
        /// Outcome of the collectDigits recognition ( if any was specified in the action )
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public CollectDigitsOutcome CollectDigitsOutcome { get; set; }

        public RecognizeOutcome()
        {
            this.Type = ValidOutcomes.RecognizeOutcome;
        }

        public override void Validate()
        {
            base.Validate();
            if (this.Outcome == Outcome.Success)
            {
                bool choiceOutcome = (this.ChoiceOutcome != null);
                bool collectDigitsOutcome = (this.CollectDigitsOutcome != null);

                Utils.AssertArgument(
                    (choiceOutcome && !collectDigitsOutcome) || (!choiceOutcome && collectDigitsOutcome),
                    "Either a ChoiceOutcome or a CollectDigitsOutcome must for specified for successful recognition outcome");

                if (choiceOutcome)
                {
                    Utils.AssertArgument(!String.IsNullOrWhiteSpace(this.ChoiceOutcome.ChoiceName),
                        "Recognized choice name must be specified for successful choice recognition outcome");
                }

                if (collectDigitsOutcome)
                {
                    Utils.AssertArgument(!String.IsNullOrWhiteSpace(this.CollectDigitsOutcome.Digits),
                        "Collected digits must be specified for successful choice recognition outcome");
                    ValidDtmfs.Validate(this.CollectDigitsOutcome.Digits.ToCharArray());
                }
            }
        }
    }
}
