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

using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Microsoft.Bot.Builder.Calling.Events;
using Microsoft.Bot.Builder.Calling.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Bot.Builder.Calling
{
    public class CallingBotService : ICallingBotService
    {
        private readonly string _callbackUrl;

        #region Implementation of ICallingBotService

        /// <summary>
        /// Event raised when bot receives incoming call
        /// </summary>
        public event Func<IncomingCallEvent, Task> OnIncomingCallReceived;

        /// <summary>
        /// Event raised when the bot gets the outcome of Answer action. If the operation was successful the call is established
        /// </summary>
        public event Func<AnswerOutcomeEvent, Task> OnAnswerCompleted;

        /// <summary>
        /// Event raised when the bot gets the outcome of Hangup action
        /// </summary>
        public event Func<HangupOutcomeEvent, Task> OnHangupCompleted;

        /// <summary>
        /// Event raised when the bot gets the outcome of PlayPrompt action
        /// </summary>
        public event Func<PlayPromptOutcomeEvent, Task> OnPlayPromptCompleted;

        /// <summary>
        /// Event raised when the bot gets the outcome of Recognize action
        /// </summary>
        public event Func<RecognizeOutcomeEvent, Task> OnRecognizeCompleted;

        /// <summary>
        /// Event raised when the bot gets the outcome of Record action
        /// </summary>
        public event Func<RecordOutcomeEvent, Task> OnRecordCompleted;

        /// <summary>
        /// Event raised when the bot gets the outcome of Reject action
        /// </summary>
        public event Func<RejectOutcomeEvent, Task> OnRejectCompleted;

        /// <summary>
        /// Event raised when specified workflow fails to be validated by Bot platform
        /// </summary>
        public event Func<WorkflowValidationOutcomeEvent, Task> OnWorkflowValidationFailed;

        /// <summary>
        ///     Instantiates CallingBotService using provided settings
        /// </summary>
        /// <param name="settings"></param>
        public CallingBotService(CallingBotServiceSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _callbackUrl = settings.CallbackUrl;
        }

        /// <summary>
        /// Method responsible for processing the data sent with POST request to callback URL
        /// </summary>
        /// <param name="content">The content of request</param>
        /// <param name="additionalData">The remaining part of request in case of multi part request</param>
        /// <returns>Returns the response that should be sent to the sender of POST request</returns>
        public async Task<string> ProcessCallbackAsync(string content, Task<Stream> additionalData)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            ConversationResult conversationResult = ConvertToConversationResult(content);
            return await ProcessConversationResult(conversationResult, additionalData).ConfigureAwait(false);
        }

        /// <summary>
        /// Method responsible for processing the data sent with POST request to incoming call URL
        /// </summary>
        /// <param name="content">The content of request</param>
        /// <returns>Returns the response that should be sent to the sender of POST request</returns>
        public string ProcessIncomingCall(string content)
        {
            return Task.Factory.StartNew(s => ((ICallingBotService)s).ProcessIncomingCallAsync(content), this, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Method responsible for processing the data sent with POST request to incoming call URL
        /// </summary>
        /// <param name="content">The content of request</param>
        /// <returns>Returns the response that should be sent to the sender of POST request</returns>
        public async Task<string> ProcessIncomingCallAsync(string content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            var conversation = Serializer.DeserializeFromJson<Conversation>(content);
            conversation.Validate();
            var workflow = await HandleIncomingCall(conversation).ConfigureAwait(false);
            if (workflow == null)
                throw new BotCallingServiceException("Incoming call not handled. No workflow produced for incoming call.");
            workflow.Validate();
            var serializedResponse = Serializer.SerializeToJson(workflow);
            return serializedResponse;
        }

        /// <summary>
        /// Method responsible for processing the data sent with POST request to callback URL
        /// </summary>
        /// <param name="content">The content of request</param>
        /// <param name="additionalData">The remaining part of request in case of multi part request</param>
        /// <returns>Returns the response that should be sent to the sender of POST request</returns>
        public string ProcessCallback(string content, Task<Stream> additionalData)
        {
            return Task.Factory.StartNew(s => ((ICallingBotService)s).ProcessCallbackAsync(content, additionalData), this, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
        }

        private async Task<string> ProcessConversationResult(ConversationResult conversationResult, Task<Stream> additionalData)
        {
            conversationResult.Validate();
            var newWorkflowResult = await PassActionResultToHandler(conversationResult, additionalData).ConfigureAwait(false);
            if (newWorkflowResult == null)
                return "";
            newWorkflowResult.Validate();
            return Serializer.SerializeToJson(newWorkflowResult);
        }

        private Task<Workflow> PassActionResultToHandler(ConversationResult receivedConversationResult, Task<Stream> additionalData)
        {
            Trace.TraceInformation(
                $"CallingBotService: Received the outcome for {receivedConversationResult.OperationOutcome.Type} operation, callId: {receivedConversationResult.OperationOutcome.Id}");

            switch (receivedConversationResult.OperationOutcome.Type)
            {
                case ValidOutcomes.AnswerOutcome:
                    return HandleAnswerOutcome(receivedConversationResult, receivedConversationResult.OperationOutcome as AnswerOutcome);
                case ValidOutcomes.HangupOutcome:
                    return HandleHangupOutcome(receivedConversationResult, receivedConversationResult.OperationOutcome as HangupOutcome);
                case ValidOutcomes.PlayPromptOutcome:
                    return HandlePlayPromptOutcome(receivedConversationResult, receivedConversationResult.OperationOutcome as PlayPromptOutcome);
                case ValidOutcomes.RecognizeOutcome:
                    return HandleRecognizeOutcome(receivedConversationResult, receivedConversationResult.OperationOutcome as RecognizeOutcome);
                case ValidOutcomes.RecordOutcome:
                    return HandleRecordOutcome(receivedConversationResult, receivedConversationResult.OperationOutcome as RecordOutcome, additionalData);
                case ValidOutcomes.RejectOutcome:
                    return HandleRejectOutcome(receivedConversationResult, receivedConversationResult.OperationOutcome as RejectOutcome);
                case ValidOutcomes.WorkflowValidationOutcome:
                    return HandleWorkflowValidationOutcome(receivedConversationResult, receivedConversationResult.OperationOutcome as WorkflowValidationOutcome);
            }
            throw new BotCallingServiceException($"Unknown conversation result type {receivedConversationResult.OperationOutcome.Type}");
        }

        private async Task<Workflow> HandleIncomingCall(Conversation conversation)
        {
            Trace.TraceInformation($"CallingBotService: Received incoming call, callId: {conversation.Id}");
            var incomingCall = new IncomingCallEvent(conversation, CreateInitialWorkflow());
            var eventHandler = OnIncomingCallReceived;
            if (eventHandler != null)
                await eventHandler.Invoke(incomingCall).ConfigureAwait(false);
            else
            {
                Trace.TraceInformation($"CallingBotService: No handler specified for incoming call");
                return null;
            }

            return incomingCall.ResultingWorkflow;
        }

        private Task<Workflow> HandleAnswerOutcome(ConversationResult conversationResult, AnswerOutcome answerOutcome)
        {
            var outcomeEvent = new AnswerOutcomeEvent(conversationResult, CreateInitialWorkflow(), answerOutcome);
            var eventHandler = OnAnswerCompleted;
            return InvokeHandlerIfSet(eventHandler, outcomeEvent);
        }

        private Task<Workflow> HandleHangupOutcome(ConversationResult conversationResult, HangupOutcome hangupOutcome)
        {
            var outcomeEvent = new HangupOutcomeEvent(conversationResult, CreateInitialWorkflow(), hangupOutcome);
            var eventHandler = OnHangupCompleted;
            return InvokeHandlerIfSet(eventHandler, outcomeEvent);
        }

        private Task<Workflow> HandlePlayPromptOutcome(ConversationResult conversationResult, PlayPromptOutcome playPromptOutcome)
        {
            var outcomeEvent = new PlayPromptOutcomeEvent(conversationResult, CreateInitialWorkflow(), playPromptOutcome);
            var eventHandler = OnPlayPromptCompleted;
            return InvokeHandlerIfSet(eventHandler, outcomeEvent);
        }

        private Task<Workflow> HandleRecognizeOutcome(ConversationResult conversationResult, RecognizeOutcome recognizeOutcome)
        {
            var outcomeEvent = new RecognizeOutcomeEvent(conversationResult, CreateInitialWorkflow(), recognizeOutcome);
            var eventHandler = OnRecognizeCompleted;
            return InvokeHandlerIfSet(eventHandler, outcomeEvent);
        }

        private Task<Workflow> HandleRecordOutcome(ConversationResult conversationResult, RecordOutcome recordOutcome, Task<Stream> recordedContent)
        {
            var outcomeEvent = new RecordOutcomeEvent(conversationResult, CreateInitialWorkflow(), recordOutcome, recordedContent);
            var eventHandler = OnRecordCompleted;
            return InvokeHandlerIfSet(eventHandler, outcomeEvent);
        }

        private Task<Workflow> HandleRejectOutcome(ConversationResult conversationResult, RejectOutcome rejectOutcome)
        {
            var outcomeEvent = new RejectOutcomeEvent(conversationResult, CreateInitialWorkflow(), rejectOutcome);
            var eventHandler = OnRejectCompleted;
            return InvokeHandlerIfSet(eventHandler, outcomeEvent);
        }

        private Task<Workflow> HandleWorkflowValidationOutcome(
            ConversationResult conversationResult,
            WorkflowValidationOutcome workflowValidationOutcome)
        {

            var outcomeEvent = new WorkflowValidationOutcomeEvent(conversationResult, CreateInitialWorkflow(), workflowValidationOutcome);
            var eventHandler = OnWorkflowValidationFailed;
            return InvokeHandlerIfSet(eventHandler, outcomeEvent);
        }

        private async Task<Workflow> InvokeHandlerIfSet<T>(Func<T, Task> action, T outcomeEventBase) where T : OutcomeEventBase
        {
            if (action != null)
            {
                await action.Invoke(outcomeEventBase).ConfigureAwait(false);
                return outcomeEventBase.ResultingWorkflow;
            }
            throw new BotCallingServiceException($"No event handler set for {outcomeEventBase.ConversationResult.OperationOutcome.Type} outcome");
        }

        private Workflow CreateInitialWorkflow()
        {
            var workflow = new Workflow();
            workflow.Links = new CallbackLink { Callback = GetCallbackUri() };
            workflow.Actions = new List<ActionBase>();

            return workflow;
        }

        private Uri GetCallbackUri()
        {
            return new Uri(_callbackUrl);
        }

        private static ConversationResult ConvertToConversationResult(string content)
        {
            try
            {
                var conversationResult = Serializer.DeserializeFromJson<ConversationResult>(content);
                return conversationResult;
            }
            catch (Exception e)
            {
                throw new BotCallingServiceException("Failed to deserialize Calling Service callback content", e);
            }
        }

        #endregion
    }
}
