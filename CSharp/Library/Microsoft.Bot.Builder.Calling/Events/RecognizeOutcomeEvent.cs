using System;

using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;

namespace Microsoft.Bot.Builder.Calling.Events
{
    public class RecognizeOutcomeEvent : OutcomeEventBase
    {
        public RecognizeOutcomeEvent(ConversationResult conversationResult, Workflow resultingWorkflow, RecognizeOutcome outcome)
            : base(conversationResult, resultingWorkflow)
        {
            if (outcome == null)
                throw new ArgumentNullException(nameof(outcome));
            RecognizeOutcome = outcome;
        }

        public RecognizeOutcome RecognizeOutcome { get; set; }
    }
}
