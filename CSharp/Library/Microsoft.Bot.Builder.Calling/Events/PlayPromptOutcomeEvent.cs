using System;

using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;

namespace Microsoft.Bot.Builder.Calling.Events
{
    public class PlayPromptOutcomeEvent : OutcomeEventBase
    {
        public PlayPromptOutcomeEvent(ConversationResult conversationResult, Workflow resultingWorkflow, PlayPromptOutcome outcome)
            : base(conversationResult, resultingWorkflow)
        {
            if (outcome == null) throw new ArgumentNullException(nameof(outcome));
            PlayPromptOutcome = outcome;
        }

        public PlayPromptOutcome PlayPromptOutcome { get; set; }
    }
}
