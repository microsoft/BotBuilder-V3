using System;

using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;

namespace Microsoft.Bot.Builder.Calling.Events
{
    public class WorkflowValidationOutcomeEvent : OutcomeEventBase
    {
        public WorkflowValidationOutcomeEvent(
            ConversationResult conversationResult,
            Workflow resultingWorkflow,
            WorkflowValidationOutcome outcome) : base(conversationResult, resultingWorkflow)
        {
            if (outcome == null) throw new ArgumentNullException(nameof(outcome));
            WorkflowValidationOutcome = outcome;
        }

        public WorkflowValidationOutcome WorkflowValidationOutcome { get; set; }
    }
}
