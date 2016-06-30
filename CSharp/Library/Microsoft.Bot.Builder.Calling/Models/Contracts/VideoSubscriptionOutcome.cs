using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the outcome of the "videoSubscription" action. 
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class VideoSubscriptionOutcome : OperationOutcomeBase
    {
        public VideoSubscriptionOutcome()
        {
            this.Type = ValidOutcomes.VideoSubscriptionOutcome;
        }
    }
}
