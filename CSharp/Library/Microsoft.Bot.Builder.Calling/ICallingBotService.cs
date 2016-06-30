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
