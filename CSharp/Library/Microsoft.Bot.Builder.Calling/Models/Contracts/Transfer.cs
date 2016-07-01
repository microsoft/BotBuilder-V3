using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is the action which customers can specify to indicate that the server call agent should transfer established
    /// call. The transfer is attended - meaning if the transfer fails the agent is able to still interact with caller.
    /// If transfer succeeds the call is automatically hang up.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Transfer : ActionBase
    {
        /// <summary>
        /// The Skype identifier of transfer target.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Participant Target { get; set; }

        public Transfer()
        {
            this.Action = ValidActions.TransferAction;
        }

        public override void Validate()
        {
            base.Validate();
            Utils.AssertArgument(this.Target != null, "Target cannot be null");
            this.Target.Validate();
            Utils.AssertArgument(this.Target.Identity.StartsWith("8:"), "Target identity has to be 8:* Skype Identity");
            Utils.AssertArgument(this.Target.Originator == false, "Target originator has to be set to false");
        }
    }
}
