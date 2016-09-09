using System;

using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;

namespace Microsoft.Bot.Builder.Calling.Events
{
    public class AnswerOutcomeEvent : OutcomeEventBase
    {
        public AnswerOutcomeEvent(ConversationResult conversationResult, Workflow resultingWorkflow, AnswerOutcome outcome) : base(conversationResult, resultingWorkflow)
        {
            if (outcome == null)
                throw new ArgumentNullException(nameof(outcome));
            AnswerOutcome = outcome;
        }

        public AnswerOutcome AnswerOutcome { get; set; }
    }
}
