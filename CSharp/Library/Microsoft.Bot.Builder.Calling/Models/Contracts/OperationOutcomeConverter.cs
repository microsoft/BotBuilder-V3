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
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// By default Json.net doesn't know how to deserialize JSON data into Interfaces or abstract classes.
    /// This custom Converter helps deserialize "operationOutcomes" specified in JSON into respective concrete "OperationOutcome" classes.
    /// </summary>
    public class OperationOutcomeConverter : JsonCreationConverter<OperationOutcomeBase>
    {
        protected override OperationOutcomeBase Create(Type objectType, JObject jsonObject)
        {
            var type = (string)jsonObject.Property("type");
            if (String.Equals(type, ValidOutcomes.AnswerOutcome, StringComparison.OrdinalIgnoreCase))
            {
                return new AnswerOutcome();
            }
            else if (String.Equals(type, ValidOutcomes.HangupOutcome, StringComparison.OrdinalIgnoreCase))
            {
                return new HangupOutcome();
            }
            else if (String.Equals(type, ValidOutcomes.RejectOutcome, StringComparison.OrdinalIgnoreCase))
            {
                return new RejectOutcome();
            }
            else if (String.Equals(type, ValidOutcomes.PlayPromptOutcome, StringComparison.OrdinalIgnoreCase))
            {
                return new PlayPromptOutcome();
            }
            else if (String.Equals(type, ValidOutcomes.RecordOutcome, StringComparison.OrdinalIgnoreCase))
            {
                return new RecordOutcome();
            }
            else if (String.Equals(type, ValidOutcomes.RecognizeOutcome, StringComparison.OrdinalIgnoreCase))
            {
                return new RecognizeOutcome();
            }
            else if (String.Equals(type, ValidOutcomes.WorkflowValidationOutcome, StringComparison.OrdinalIgnoreCase))
            {
                return new WorkflowValidationOutcome();
            }

            throw new ArgumentException(String.Format("The given outcome type '{0}' is not supported!", type));
        }
    }
}
