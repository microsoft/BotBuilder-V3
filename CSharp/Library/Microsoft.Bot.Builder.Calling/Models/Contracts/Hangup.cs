using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the action which customers can specify to indicate that the server call agent should hangup the call.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Hangup : ActionBase
    {
        public Hangup()
        {
            this.Action = ValidActions.HangupAction;
        }
    }
}
