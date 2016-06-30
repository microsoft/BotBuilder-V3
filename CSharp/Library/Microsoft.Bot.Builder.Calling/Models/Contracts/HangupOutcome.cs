using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the outcome of the "hangup" action. This is conveyed to the customer as POST to the customer CallBack Url (if specified).
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class HangupOutcome : OperationOutcomeBase
    {
        public HangupOutcome()
        {
            this.Type = ValidOutcomes.HangupOutcome;
        }
    }
}
