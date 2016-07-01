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
        /// AnswerAppHostedMediaOutcome
        /// </summary>
        public const string AnswerAppHostedMediaOutcome = "answerAppHostedMediaOutcome";

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
        /// PlaceCallOutcome
        /// </summary>
        public const string PlaceCallOutcome = "placeCallOutcome";

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
        /// VideoSubscriptionOutcome
        /// </summary>
        public const string VideoSubscriptionOutcome = "videoSubscriptionOutcome";

        /// <summary>
        /// Attended transfer outcome.
        /// </summary>
        public const string TransferOutcome = "transferOutcome";

        /// <summary>
        /// list of valid outcomes
        /// </summary>
        private static HashSet<string> validOutcomes = new HashSet<string>()
        {
            AnswerOutcome,
            AnswerAppHostedMediaOutcome,
            HangupOutcome,
            RejectOutcome,
            PlaceCallOutcome,
            PlayPromptOutcome,
            RecordOutcome,
            RecognizeOutcome,
            WorkflowValidationOutcome,
            VideoSubscriptionOutcome,
            TransferOutcome,
        };

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
