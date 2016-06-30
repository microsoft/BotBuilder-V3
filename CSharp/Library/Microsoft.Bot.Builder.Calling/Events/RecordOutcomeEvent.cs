using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;

namespace Microsoft.Bot.Builder.Calling.Events
{
    public class RecordOutcomeEvent : OutcomeEventBase
    {
        public RecordOutcomeEvent(
            ConversationResult conversationResult,
            Workflow resultingWorkflow,
            RecordOutcome outcome,
            Task<Stream> recordedContent) : base(conversationResult, resultingWorkflow)
        {
            if (outcome == null) throw new ArgumentNullException(nameof(outcome));
            RecordOutcome = outcome;
            RecordedContent = recordedContent;
        }

        public RecordOutcome RecordOutcome { get; set; }

        public Task<Stream> RecordedContent { get; set; }
    }
}