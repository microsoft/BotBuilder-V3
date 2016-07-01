using System;

using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;

namespace Microsoft.Bot.Builder.Calling.Events
{
    public class IncomingCallEvent
    {
        public IncomingCallEvent(Conversation conversation, Workflow resultingWorkflow)
        {
            if (conversation == null)
                throw new ArgumentNullException(nameof(conversation));
            if (resultingWorkflow == null)
                throw new ArgumentNullException(nameof(resultingWorkflow));
            IncomingCall = conversation;
            ResultingWorkflow = resultingWorkflow;
        }

        public Conversation IncomingCall { get; set; }

        public Workflow ResultingWorkflow { get; set; }
    }
}
