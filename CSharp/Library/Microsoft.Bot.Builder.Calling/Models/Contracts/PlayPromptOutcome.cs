using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the outcome of the "playPrompt" action. This is conveyed to the customer as POST to the customer CallBack Url.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class PlayPromptOutcome : OperationOutcomeBase
    {
        public PlayPromptOutcome()
        {
            this.Type = ValidOutcomes.PlayPromptOutcome;
        }
    }
}
