using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// If the customer's "response" fails validation, this is the outcome conveyed to the customer as POST to the customer CallBack Url.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class WorkflowValidationOutcome : OperationOutcomeBase
    {
        public WorkflowValidationOutcome()
        {
            this.Type = ValidOutcomes.WorkflowValidationOutcome;
        }
    }
}
