// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// An activity by which a bot can log internal information into a logged conversation transcript
    /// </summary>
    public interface ITraceActivity : IActivity
    {
        /// <summary>
        /// Name of the trace activity 
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Descriptive label for the trace
        /// </summary>
        string Label { get; set; }

        /// <summary>
        /// Unique string which identifies the format of the value object
        /// </summary>
        string ValueType { get; set; }

        /// <summary>
        /// Open-ended value
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Reference to another conversation or activity
        /// </summary>
        ConversationReference RelatesTo { get; set; }
    }
}
