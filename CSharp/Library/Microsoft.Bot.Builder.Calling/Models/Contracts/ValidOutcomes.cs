// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is a helper class for validating outcomes. This can be used by customers or by us (before we send the outcome on the wire) 
    /// </summary>
    public static class ValidOutcomes
    {
        /// <summary>
        /// AnswerOutcome
        /// </summary>
        public const string AnswerOutcome = "answerOutcome";

        /// <summary>
        /// HangupOutcome
        /// </summary>
        public const string HangupOutcome = "hangupOutcome";

        /// <summary>
        /// RejectOutcome
        /// </summary>
        public const string RejectOutcome = "rejectOutcome";

        /// <summary>
        /// PlayPromptOutcome
        /// </summary>
        public const string PlayPromptOutcome = "playPromptOutcome";

        /// <summary>
        /// RecordOutcome
        /// </summary>
        public const string RecordOutcome = "recordOutcome";

        /// <summary>
        /// RecognizeOutcome
        /// </summary>
        public const string RecognizeOutcome = "recognizeOutcome";

        /// <summary>
        /// WorkflowValidationOutcome
        /// </summary>
        public const string WorkflowValidationOutcome = "worfklowValidationOutcome";
      
        /// <summary>
        /// list of valid outcomes
        /// </summary>
        private static HashSet<string> validOutcomes = new HashSet<string>()
        {
            AnswerOutcome,
            HangupOutcome,
            RejectOutcome,
            PlayPromptOutcome,
            RecordOutcome,
            RecognizeOutcome,
            WorkflowValidationOutcome,
        };

        public static HashSet<string> Outcomes
        {
            get { return validOutcomes; }
        }

        public static void Validate(string outcome)
        {
            Utils.AssertArgument(!String.IsNullOrWhiteSpace(outcome), "Outcome cannot be null or empty");
            Utils.AssertArgument(validOutcomes.Contains(outcome), "{0} is not a valid outcome type", outcome);
        }

        public static void Validate(OperationOutcomeBase operationOutcome)
        {
            Utils.AssertArgument(operationOutcome != null, "operationOutcome cannot be null");
            operationOutcome.Validate();
        }
    }
}
