using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the outcome of the "transfer" action. This is conveyed to the customer as POST to the customer CallBack Url. 
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class TransferOutcome : OperationOutcomeBase
    {
        public TransferOutcome()
        {
            this.Type = ValidOutcomes.TransferOutcome;
        }

        // Outcome flag indicates transfer status. 
        // Outcome.Success means user was transfered successfully and current call was hangup.
        // While Outcome.Failure means that the transfer failed.
    }
}
