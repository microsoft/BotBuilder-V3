using System;

using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;

namespace Microsoft.Bot.Builder.Calling.Events
{
    public class RejectOutcomeEvent : OutcomeEventBase
    {
        public RejectOutcomeEvent(ConversationResult conversationResult, Workflow resultingWorkflow, RejectOutcome outcome)
            : base(conversationResult, resultingWorkflow)
        {
            if (outcome == null) throw new ArgumentNullException(nameof(outcome));
            RejectOutcome = outcome;
        }

        public RejectOutcome RejectOutcome { get; set; }
    }
}
