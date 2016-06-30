using System;

using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;

namespace Microsoft.Bot.Builder.Calling.Events
{
    public class HangupOutcomeEvent : OutcomeEventBase
    {
        public HangupOutcomeEvent(ConversationResult conversationResult, Workflow resultingWorkflow, HangupOutcome outcome)
            : base(conversationResult, resultingWorkflow)
        {
            if (outcome == null)
                throw new ArgumentNullException(nameof(outcome));
            HangupOutcome = outcome;
        }

        public HangupOutcome HangupOutcome { get; set; }
    }
}
