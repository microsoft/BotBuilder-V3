using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the outcome of the "record" action. This is conveyed to the customer as POST to the customer CallBack Url. 
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class RecordOutcome : OperationOutcomeBase
    {
        /// <summary>
        /// Completion reason
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public RecordingCompletionReason CompletionReason { get; set; }

        /// <summary>
        /// If recording was successful, this indicates length of recorded audio
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public double LengthOfRecordingInSecs { get; set; }

        /// <summary>
        /// Media encoding format of the recording.
        /// </summary>
        public RecordingFormat Format { get; set; }

        public RecordOutcome()
        {
            this.Type = ValidOutcomes.RecordOutcome;
        }

        public override void Validate()
        {
            base.Validate();
            if (this.Outcome == Outcome.Success)
            {
                Utils.AssertArgument(this.LengthOfRecordingInSecs > 0,
                    "Recording Length must be specified for successful recording");
            }
            else
            {
                Utils.AssertArgument(this.LengthOfRecordingInSecs <= 0,
                    "Recording Length must not be specified for failed recording");
            }
        }
    }
}
