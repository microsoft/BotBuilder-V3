using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder
{
    [Serializable]
    public sealed class TaskNotCompletedException : Exception
    {
        public TaskNotCompletedException(string message)
            : base(message)
        {
        }
        private TaskNotCompletedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public sealed class StackDisciplineException : Exception
    {
        public StackDisciplineException(string message)
            : base(message)
        {
        }
        private StackDisciplineException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class DialogException : Exception
    {
        public readonly IDialog Dialog;

        public DialogException(string message, IDialog dialog)
            : base(message)
        {
            this.Dialog = dialog;
        }

        private DialogException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public sealed class InvalidSessionException : Exception
    {
        public InvalidSessionException(string message)
            : base(message)
        {
        }
        private InvalidSessionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}