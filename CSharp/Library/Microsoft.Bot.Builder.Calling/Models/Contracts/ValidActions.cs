using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is a helper class for validating actions specified by customers
    /// </summary>
    public static class ValidActions
    {
        /// <summary>
        /// AnswerAction
        /// </summary>
        public const string AnswerAction = "answer";

        /// <summary>
        /// AnswerAppHostedMediaAction
        /// </summary>
        public const string AnswerAppHostedMediaAction = "answerAppHostedMedia";

        /// <summary>
        /// HangupAction
        /// </summary>
        public const string HangupAction = "hangup";

        /// <summary>
        /// PlayPromptAction
        /// </summary>
        public const string PlayPromptAction = "playPrompt";

        /// <summary>
        /// RecordAction
        /// </summary>
        public const string RecordAction = "record";

        /// <summary>
        /// RecognizeAction
        /// </summary>
        public const string RecognizeAction = "recognize";

        /// <summary>
        /// RejectAction
        /// </summary>
        public const string RejectAction = "reject";

        /// <summary>
        /// PlaceCall
        /// </summary>
        public const string PlaceCallAction = "placeCall";

        /// <summary>
        /// VideoSubscription
        /// </summary>
        public const string VideoSubscriptionAction = "videoSubscription";

        /// <summary>
        /// Attended transfer.
        /// </summary>
        public const string TransferAction = "transfer";

        /// <summary>
        /// Dictionary of valid actions and their relative order
        /// +ve order reflect operations after and including call acceptance
        /// -ve order reflect operations pre-call answering . ex: reject/redirect/simulRing/sequentialRing
        /// </summary>
        private static Dictionary<string, int> actionsList = new Dictionary<string, int>()
        {
            {RejectAction, -2},
            {AnswerAction, 1},
            {AnswerAppHostedMediaAction, 1},
            {PlaceCallAction, 1},
            {VideoSubscriptionAction, 1},
            {PlayPromptAction, 2},
            {RecordAction, 2},
            {RecognizeAction, 2},
            {TransferAction, 2},
            {HangupAction, 3},
        };

        public static void Validate(string action)
        {
            Utils.AssertArgument(!String.IsNullOrWhiteSpace(action), "Action Name cannot be null or empty");
            Utils.AssertArgument(actionsList.ContainsKey(action), "{0} is not a valid action", action);
        }

        public static void Validate(IEnumerable<ActionBase> actions)
        {
            Utils.AssertArgument(actions != null, "Null Actions List not allowed");
            ActionBase[] actionsTobeValidated = actions.ToArray();
            Utils.AssertArgument(actionsTobeValidated.Length > 0, "Empty Actions List not allowed");

            if (actionsTobeValidated.Length > 1 && actionsTobeValidated.Any((a) => { return a.IsStandaloneAction; }))
            {
                Utils.AssertArgument(
                    false,
                    "The stand-alone action '{0}' cannot be specified with any other actions",
                    (actionsTobeValidated.FirstOrDefault((a) => { return a.IsStandaloneAction; })).Action);
            }

            int prevOrder = 0;
            int currOrder = 0;
            HashSet<string> temp = new HashSet<string>();

            ActionBase actionBase = actionsTobeValidated[0];
            Utils.AssertArgument(actionBase != null, "action cannot be null");
            string action = actionBase.Action;

            actionBase.Validate();
            bool condition = actionsList.TryGetValue(action, out prevOrder);
            Utils.AssertArgument(condition, "{0} is not a valid action", action);
            temp.Add(action);

            for (int i = 1; i < actionsTobeValidated.Length; i++)
            {
                actionBase = actionsTobeValidated[i];
                Utils.AssertArgument(actionBase != null, "action cannot be null");
                action = actionsTobeValidated[i].Action;
                actionBase.Validate();
                condition = actionsList.TryGetValue(action, out currOrder);
                Utils.AssertArgument(condition, "{0} is not a valid action", action);
                Utils.AssertArgument((currOrder >= prevOrder) && (Math.Sign(currOrder) == Math.Sign(prevOrder)),
                    "Action : {0} violates action ordering requirement",
                    action);
                Utils.AssertArgument(!temp.Contains(action), "Action : {0} cannot be specified multiple times in the same response", action);

                temp.Add(action);
                prevOrder = currOrder;
            }

            Utils.AssertArgument(!(temp.Contains(AnswerAction) && temp.Contains(PlaceCallAction)), "Both Answer and PlaceCall cannot be specified");
        }
    }
}
