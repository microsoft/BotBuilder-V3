using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This class contains the workflow the customer sent for the OnInComingCall POST or any subsequent POST to their callback url.
    /// Basically this workflow defines the set of actions, the customer wants us to perform and then callback to them.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Workflow
    {
        /// <summary>
        /// Callback link to call back the customer on, once we have performed the set of actions.
        /// 
        /// Note : 
        /// a. We would always return the outcome :
        ///     i. of the last operation if all operations were performed successfully OR
        ///     ii. outcome of first failed operation 
        /// b. If any operation fails, then we immediately callback the customer webservice with the outcome, 
        ///     and skip processing other operations defined in the "actions" list. 
        /// c. If no callback link is provided, then we keep performing all specified operations, until 
        ///     i. we hit the end - then we hangup (if call connected to server call agent)
        ///     ii. We hit a failure - then we hangup (if call connected to server call agent)
        ///     iii. We hit a max call duration timeout - then we hangup (if call connected to server call agent)
        /// d. Any validation failure of this workflow object would result in us returning
        ///    the workflowValidationOutcome object to the customer's callback url and not proceed with any defined actions.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public CallbackLink Links { get; set; }

        /// <summary>
        /// List of actions to perform . ex : playPrompt, record, hangup
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public IEnumerable<ActionBase> Actions { get; set; }

        /// <summary>
        /// Opaque string to facilitate app developers to pass their custom data in this field. 
        /// This field is echo'd back in the 'result' POST for this 'workflow'.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string AppState { get; set; }

        /// <summary>
        /// This element indicates that application wants to receive notification updates. 
        /// Call state notifications are added to this list by default and cannot be unsubscribed to.
        /// Subscriptions to rosterUpdate are only used for multiParty calls.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public IEnumerable<NotificationType> NotificationSubscriptions { get; set; }

        public void Validate()
        {
            ValidActions.Validate(this.Actions);
            if (this.Links != null)
            {
                Utils.AssertArgument(this.Links.Callback != null, "Callback link cannot be specified as null");
                Utils.AssertArgument(this.Links.Callback.IsAbsoluteUri, "Callback link must be an absolute uri");
            }
            ApplicationState.Validate(this.AppState);

            if (this.NotificationSubscriptions != null)
            {
                if (!this.NotificationSubscriptions.Contains<NotificationType>(NotificationType.CallStateChange))
                {
                    List<NotificationType> newNotificationSubscriptionList = new List<NotificationType>();
                    newNotificationSubscriptionList.Add(NotificationType.CallStateChange);
                    newNotificationSubscriptionList.AddRange(this.NotificationSubscriptions);
                    this.NotificationSubscriptions = newNotificationSubscriptionList;
                }
            }
            else
            {
                List<NotificationType> callStateNotificationList = new List<NotificationType>();
                callStateNotificationList.Add(NotificationType.CallStateChange);
                this.NotificationSubscriptions = callStateNotificationList;
            }
        }
    }
}
