using System;

using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;

namespace Microsoft.Bot.Builder.Calling.Events
{
    public abstract class OutcomeEventBase
    {
        public OutcomeEventBase(ConversationResult conversationResult, Workflow resultingWorkflow)
        {
            if (conversationResult == null)
                throw new ArgumentNullException(nameof(conversationResult));
            if (resultingWorkflow == null)
                throw new ArgumentNullException(nameof(resultingWorkflow));
            ConversationResult = conversationResult;
            ResultingWorkflow = resultingWorkflow;
        }

        public ConversationResult ConversationResult { get; set; }

        public Workflow ResultingWorkflow { get; set; }
    }
}
