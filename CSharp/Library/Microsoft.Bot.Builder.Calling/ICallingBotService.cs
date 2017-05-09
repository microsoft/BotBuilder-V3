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
using System.IO;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Calling.Events;

namespace Microsoft.Bot.Builder.Calling
{
    public interface ICallingBotService
    {
        /// <summary>
        /// Event raised when bot receives incoming call
        /// </summary>
        event Func<IncomingCallEvent, Task> OnIncomingCallReceived;

        /// <summary>
        /// Event raised when the bot gets the outcome of Answer action. If the operation was successful the call is established
        /// </summary>
        event Func<AnswerOutcomeEvent, Task> OnAnswerCompleted;

        /// <summary>
        /// Event raised when the bot gets the outcome of Hangup action
        /// </summary>
        event Func<HangupOutcomeEvent, Task> OnHangupCompleted;

        /// <summary>
        /// Event raised when the bot gets the outcome of PlayPrompt action
        /// </summary>
        event Func<PlayPromptOutcomeEvent, Task> OnPlayPromptCompleted;

        /// <summary>
        /// Event raised when the bot gets the outcome of Recognize action
        /// </summary>
        event Func<RecognizeOutcomeEvent, Task> OnRecognizeCompleted;

        /// <summary>
        /// Event raised when the bot gets the outcome of Record action
        /// </summary>
        event Func<RecordOutcomeEvent, Task> OnRecordCompleted;

        /// <summary>
        /// Event raised when the bot gets the outcome of Reject action
        /// </summary>
        event Func<RejectOutcomeEvent, Task> OnRejectCompleted;

        /// <summary>
        /// Event raised when specified workflow fails to be validated by Bot platform
        /// </summary>
        event Func<WorkflowValidationOutcomeEvent, Task> OnWorkflowValidationFailed;

        /// <summary>
        /// Method responsible for processing the data sent with POST request to callback URL
        /// </summary>
        /// <param name="content">The content of request</param>
        /// <param name="additionalData">The remaining part of request in case of multi part request</param>
        /// <returns>Returns the response that should be sent to the sender of POST request</returns>
        string ProcessCallback(string content, Task<Stream> additionalData);

        /// <summary>
        /// Method responsible for processing the data sent with POST request to callback URL
        /// </summary>
        /// <param name="content">The content of request</param>
        /// <param name="additionalData">The remaining part of request in case of multi part request</param>
        /// <returns>Returns the response that should be sent to the sender of POST request</returns>
        Task<string> ProcessCallbackAsync(string content, Task<Stream> additionalData);

        /// <summary>
        /// Method responsible for processing the data sent with POST request to incoming call URL
        /// </summary>
        /// <param name="content">The content of request</param>
        /// <returns>Returns the response that should be sent to the sender of POST request</returns>
        string ProcessIncomingCall(string content);

        /// <summary>
        /// Method responsible for processing the data sent with POST request to incoming call URL
        /// </summary>
        /// <param name="content">The content of request</param>
        /// <returns>Returns the response that should be sent to the sender of POST request</returns>
        Task<string> ProcessIncomingCallAsync(string content);
    }
}
