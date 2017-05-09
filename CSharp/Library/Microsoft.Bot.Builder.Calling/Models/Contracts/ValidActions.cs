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
        /// Dictionary of valid actions and their relative order
        /// +ve order reflect operations after and including call acceptance
        /// -ve order reflect operations pre-call answering . ex: reject/redirect/sequentialRing
        /// </summary>
        private readonly static Dictionary<string, int> actionOrder = new Dictionary<string, int>()
        {
            {RejectAction, -2},
            {AnswerAction, 1},
            {PlayPromptAction, 2},
            {RecordAction, 2},
            {RecognizeAction, 2},
            {HangupAction, 3},
        };

        private static readonly string[] exclusiveActions = new string[]
        {
            AnswerAction,
        };

        private static readonly string[] allowedMultiActions = new string[]
        {
            PlayPromptAction
        };

        private static bool IsValidAction(string action)
        {
            return actionOrder.ContainsKey(action);
        }

        private static bool IsExclusiveAction(string action)
        {
            return exclusiveActions.Contains(action);
        }

        private static bool IsMultiAction(string action)
        {
            return allowedMultiActions.Contains(action);
        }

        private static int CompareOrder(object x, object y)
        {
            string xString = (string)x;
            string yString = (string)y;
            int xOrder = actionOrder[xString];
            int yOrder = actionOrder[yString];
            return xOrder.CompareTo(yOrder);
        }

        public static void Validate(string action)
        {
            Utils.AssertArgument(!String.IsNullOrWhiteSpace(action), "Action Name cannot be null or empty");
            Utils.AssertArgument(ValidActions.IsValidAction(action), "{0} is not a valid action", action);
        }

        public static void Validate(IEnumerable<ActionBase> actions)
        {
            Utils.AssertArgument(actions != null, "Null Actions List not allowed");
            ActionBase[] actionsToBeValidated = actions.ToArray();
            Utils.AssertArgument(actionsToBeValidated.Length > 0, "Empty Actions List not allowed");

            if (actionsToBeValidated.Length > 1 && actionsToBeValidated.Any((a) => { return a.IsStandaloneAction; }))
            {
                Utils.AssertArgument(
                    false,
                    "The stand-alone action '{0}' cannot be specified with any other actions",
                    (actionsToBeValidated.FirstOrDefault((a) => { return a.IsStandaloneAction; })).Action);
            }

            // Validate each action is correct.
            for (int i = 0; i < actionsToBeValidated.Length; ++i)
            {
                Utils.AssertArgument(actionsToBeValidated[i] != null, "action {0} cannot be null", i);
                ValidActions.Validate(actionsToBeValidated[i].Action);
                actionsToBeValidated[i].Validate();
            }

            // Validate the actions are correctly ordered.
            for (int i = 1; i < actionsToBeValidated.Length; ++i)
            {
                if (ValidActions.CompareOrder(actionsToBeValidated[i - 1].Action, actionsToBeValidated[i].Action) > 0)
                {
                    Utils.AssertArgument(false, "Action {0} can not precede action {1}.", actionsToBeValidated[i - 1].Action, actionsToBeValidated[i].Action);
                }
            }

            // Ensure that actions are not duplicated
            for (int i = 0; i < actionsToBeValidated.Length; ++i)
            {
                if (ValidActions.IsMultiAction(actionsToBeValidated[i].Action))
                {
                    continue;
                }
                int actionCount = actionsToBeValidated.Where(a => a.Action == actionsToBeValidated[i].Action).Count();
                Utils.AssertArgument(actionCount <= 1, "Action {0} can not be specified multiple times in same workflow.", actionsToBeValidated[i].Action);
            }

            // Some actions (Answer and PlaceCall) cannot be combined in one workflow.
            var exclusiveActions = actionsToBeValidated.Where(a => ValidActions.IsExclusiveAction(a.Action)).ToArray();

            if (exclusiveActions.Count() > 1)
            {
                Utils.AssertArgument(false, "Action {0} can not be specified with action {1}.", exclusiveActions[0].Action, exclusiveActions[1].Action);
            }
        }
    }
}
