using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Bot.Sample.TemplateBot
{
    /// <summary>
    /// <see cref="ActivityController"/> is a controller that handles <see cref="Activity"/> events
    /// from the Bot Framework.
    /// </summary>
    [BotAuthentication]
    public class ActivityController : ApiController
    {
        /// <summary>
        /// This is the API method that represents the Bot's URL endpoint.
        /// </summary>
        /// <param name="activity">The incoming <see cref="Activity"/> event.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous post operation.</returns>
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity, CancellationToken token)
        {
            var response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.Accepted;

            // dispatch based on the activity to the private methods below
            IDispatcher dispatcher = new ActivityControllerDispatcher(this, activity, response);
            await dispatcher.TryPostAsync(token);

            return response;
        }

        /// <remarks>
        /// <see cref="MethodBindAttribute"/> is a marker attribute to indicate that this method
        /// will participate in the dispatching logic of <see cref="Dispatcher"/> if
        /// 1. all of the method's parameters can be resolved from the incoming <see cref="Activity"/>. 
        /// </remarks>
        [MethodBind]
        [ScorableGroup(1)]
        private async Task OnConversationUpdate(IConnectorClient connector, IConversationUpdateActivity update, CancellationToken token)
        {
            if (update.MembersAdded != null)
            {
                foreach (var member in update.MembersAdded)
                {
                    var reply = CreateReply(update, $"Welcome {member.Name}!");
                    await connector.Conversations.ReplyToActivityAsync(reply, token);
                }
            }

            if (update.MembersRemoved != null)
            {
                foreach (var member in update.MembersRemoved)
                {
                    var reply = CreateReply(update, $"Farewell {member.Name}!");
                    await connector.Conversations.ReplyToActivityAsync(reply, token);
                }
            }
        }

        /// <remarks>
        /// <see cref="RegexPatternAttribute"/> is a marker attribute to indicate that this method
        /// will participate in the dispatching logic of <see cref="Dispatcher"/> if
        /// 1. the regular expression matches the incoming <see cref="IMessageActivity"/>'s text, and 
        /// 2. all of the method's parameters can be resolved from the incoming <see cref="Activity"/>. 
        /// </remarks>
        [RegexPattern(@"echo\s*(?<text>(?:.*)?)")]
        [ScorableGroup(1)]
        private async Task OnEcho(IMessageActivity message, Capture text, IConnectorClient connector, CancellationToken token)
        {
            var reply = CreateReply(message, $"echo: {text.Value}");
            await connector.Conversations.ReplyToActivityAsync(reply, token);
        }

        [MethodBind]
        [ScorableGroup(1)]
        private async Task OnInvoke(IInvokeActivity trigger, HttpResponseMessage response, CancellationToken token)
        {
            response.StatusCode = HttpStatusCode.NotImplemented;
        }

        /// <remarks>
        /// <see cref="LuisModelAttribute"/> and <see cref="LuisIntentAttribute"/> are marker attributes to indicate that this method
        /// will participate in the dispatching logic of <see cref="Dispatcher"/> if
        /// 1. the LUIS model matches the incoming <see cref="IMessageActivity"/>'s text, and 
        /// 2. all of the method's parameters can be resolved from the incoming <see cref="Activity"/>. 
        /// </remarks>
        [LuisModel("c413b2ef-382c-45bd-8ff0-f76d60e2a821", "6d0966209c6e4f6b835ce34492f3e6d9")]
        [LuisIntent("builtin.intent.alarm.set_alarm")]
        [ScorableGroup(2)]
        private async Task OnSetAlarm(IMessageActivity message, [Entity(@"builtin.alarm.start_time")]EntityRecommendation startTime, IConnectorClient connector, CancellationToken token)
        {
            var reply = CreateReply(message, $"setting an alarm for {startTime.Entity}");
            await connector.Conversations.ReplyToActivityAsync(reply, token);
        }

        private static Activity CreateReply(IActivity activity, string text)
        {
            var reply = ((Activity)activity).CreateReply();
            reply.Text = text;
            return reply;
        }
    }


    /// <summary>
    /// Specialize <see cref="Dispatcher"/> for activity controllers.
    /// </summary>
    public sealed class ActivityControllerDispatcher : Dispatcher
    {
        private readonly ApiController controller;
        private readonly Activity activity;
        private readonly HttpResponseMessage response;
        public ActivityControllerDispatcher(ApiController controller, Activity activity, HttpResponseMessage response)
        {
            SetField.NotNull(out this.controller, nameof(controller), controller);
            SetField.NotNull(out this.activity, nameof(activity), activity);
            SetField.NotNull(out this.response, nameof(response), response);
        }

        protected override Type MakeType()
        {
            return this.controller.GetType();
        }

        protected override IReadOnlyList<object> MakeServices()
        {
            var credentials = new MicrosoftAppCredentials();
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl), credentials);
            // TODO: different state storage for emulator
            var storage = new StateClient(credentials);
            return new object[] { this.controller, this.activity, this.response, connector, storage };
        }
    }
}