using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// Once we have peformed the "actions" requested by the customer, we POST back to customer callback Url with this "result" object.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class ConversationResult : ConversationBase
    {
        /// <summary>
        /// a. We would always return the outcome :
        ///     i. of the last operation if all operations were performed successfully OR
        ///     ii. outcome of first failed operation 
        /// b. If any operation fails, then we immediately callback the customer webservice with the outcome, 
        ///     and skip processing other operations defined in the "actions" list. 
        /// c. If no callback link is provided, then we keep performing all specified operations, until 
        ///     i. we hit the end - then we hangup (if call connected to server call agent)
        ///     ii. We hit a failure - then we hangup (if call connected to server call agent)
        ///     iii. We hit a max call duration timeout - then we hangup (if call connected to server call agent)
        /// d. Any validation failure of this response object would result in us returning
        ///    the WorkflowValidationOutcome object to the customer's callback url and not proceed with any defined actions.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public OperationOutcomeBase OperationOutcome { get; set; }

        /// <summary>
        /// Current state of the Call
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public CallState CallState { get; set; }

        public override void Validate()
        {
            base.Validate();
            ValidOutcomes.Validate(this.OperationOutcome);
        }
    }
}
