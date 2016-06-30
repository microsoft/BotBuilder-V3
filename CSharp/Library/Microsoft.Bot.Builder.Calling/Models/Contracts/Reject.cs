using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the action which customers can specify to indicate that the server call agent should reject the call.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Reject : ActionBase
    {
        public Reject()
            : base(isStandaloneAction: true)
        {
            this.Action = ValidActions.RejectAction;
        }
    }
}
