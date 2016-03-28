// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
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
//

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.FormFlow
{
    /// <summary>
    /// A delegate for testing a form state to see if a particular step is active.
    /// </summary>
    /// <typeparam name="T">Form state type.</typeparam>
    /// <param name="state">Form state to test.</param>
    /// <returns>True if step is active given the current form state.</returns>
    public delegate bool ConditionalDelegate<T>(T state);

    /// <summary>   Encapsulates the result of a <see cref="ValidateDelegate{T}"/> </summary>
    public struct ValidateResult
    {
        /// <summary>   Feedback to provide back to the user on the input. </summary>
        public string Feedback;

        /// <summary>   True if value is a valid response. </summary>
        public bool IsValid;
    }

    /// <summary>
    /// A delegate for validating a particular response to a prompt.
    /// </summary>
    /// <typeparam name="T">Form state type.</typeparam>
    /// <param name="state">Form state to test.</param>
    /// <param name="value">Response value to validate.</param>
    /// <returns>Null if value is valid otherwise feedback on what is wrong.</returns>
    public delegate Task<ValidateResult> ValidateDelegate<T>(T state, object value);

    /// <summary>
    /// A delegate called when a form is completed.
    /// </summary>
    /// <typeparam name="T">Form state type.</typeparam>
    /// <param name="context">Session where form dialog is taking place.</param>
    /// <param name="state">Completed form state.</param>
    /// <remarks>
    /// This delegate gives an opportunity to take an action on a completed form
    /// such as sending it to your service.  It cannot be used to create a new
    /// dialog or return a value to the parent dialog.
    /// </remarks>
    public delegate Task CompletionDelegate<T>(IDialogContext context, T state);

    /// <summary>
    /// Interface for controlling the form dialog created.
    /// </summary>
    /// <typeparam name="T">Form state type.</typeparam>
    /// <remarks>
    /// A form consists of a series of steps that can be one of:
    /// <list type="list">
    /// <item>A message to the user.</item>
    /// <item>A prompt sent to the user where the response is to fill in a form state value.</item>
    /// <item>A confirmation of the current state with the user.</item>
    /// </list>
    /// By default the steps are executed in the order of the <see cref="Message"/>, <see cref="Prompt"/> and <see cref="Confirm"/> calls.
    /// If you do not take explicit control, the steps will be executed in the order defined in the form state class with a final confirmation.
    /// </remarks>
    public interface IFormDialog<T> : IDialog
    {
        /// <summary>
        /// The form specification.
        /// </summary>
        IForm<T> Form { get; }
    }

    /// <summary>
    /// Commands supported in form dialogs.
    /// </summary>
    public enum FormCommand {
        /// <summary>
        /// Move back to the previous step.
        /// </summary>
        Backup,

        /// <summary>
        /// Ask for help on responding to the current field.
        /// </summary>
        Help,

        /// <summary>
        /// Quit filling in the current form and return failure to parent dialog.
        /// </summary>
        Quit,

        /// <summary>
        /// Reset the status of the form dialog.
        /// </summary>
        Reset,

        /// <summary>
        /// Provide feedback to the user on the current form state.
        /// </summary>
        Status };

    /// <summary>
    /// Description of all the information needed for a built-in command.
    /// </summary>
    public class CommandDescription
    {
        /// <summary>
        /// Description of the command.
        /// </summary>
        public string Description;

        /// <summary>
        /// Regexs for matching the command.
        /// </summary>
        public string[] Terms;

        /// <summary>
        /// Help string for the command.
        /// </summary>
        public string Help;

        /// <summary>
        /// Construct the description of a built-in command.
        /// </summary>
        /// <param name="description">Description of the command.</param>
        /// <param name="terms">Terms that match the command.</param>
        /// <param name="help">Help on what the command does.</param>
        public CommandDescription(string description, string[] terms, string help)
        {
            Description = description;
            Terms = terms;
            Help = help;
        }
    }
 }

