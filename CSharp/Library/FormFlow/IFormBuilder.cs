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

using Microsoft.Bot.Builder.FormFlow.Advanced;

namespace Microsoft.Bot.Builder.FormFlow
{
    #region Documentation
    /// <summary>   Interface for building a form. </summary>
    /// <remarks>   
    /// A form consists of a series of steps that can be one of:
    /// <list type="list">
    /// <item>A message to the user.</item>
    /// <item>A prompt sent to the user where the response is to fill in a form state value.</item>
    /// <item>A confirmation of the current state with the user.</item>
    /// </list>
    /// By default the steps are executed in the order of the <see cref="Message"/>, <see cref="Field"/> and <see cref="Confirm"/> calls.
    /// If you do not take explicit control, the steps will be executed in the order defined in the 
    /// form state class with a final confirmation.
    /// This interface allows you to flently build a form by composing together fields,
    /// messages and confirmation.  The fluent building blocks provide common patterns
    /// like fields being based on your state class, but you can also build up your
    /// own definition of a form by using <see cref="Field{T}"/>, <see cref="FieldReflector{T}"/>
    /// or your own implementation of <see cref="IField{T}"/>.
    /// </remarks>
    /// <typeparam name="T">    Form state. </typeparam>
    #endregion
    public interface IFormBuilder<T>
    {
        /// <summary>
        /// Build the form based on the methods called on the builder.
        /// </summary>
        /// <returns>The constructed form.</returns>
        IForm<T> Build();

        /// <summary>
        /// The form configuration supplies default templates and settings.
        /// </summary>
        /// <returns>The current form configuration.</returns>
        FormConfiguration Configuration { get; }

        /// <summary>
        /// Show a message that does not require a response.
        /// </summary>
        /// <param name="message">A \ref patterns string to fill in and send.</param>
        /// <param name="condition">Whether or not this step is active.</param>
        /// <returns>This form.</returns>
        IFormBuilder<T> Message(string message, ConditionalDelegate<T> condition = null);

        /// <summary>
        /// Show a message with more format control that does not require a response.
        /// </summary>
        /// <param name="prompt">Message to fill in and send.</param>
        /// <param name="condition">Whether or not this step is active.</param>
        /// <returns>This form.</returns>
        IFormBuilder<T> Message(PromptAttribute prompt, ConditionalDelegate<T> condition = null);

        /// <summary>
        /// Define a step for filling in a particular value in the form state.
        /// </summary>
        /// <param name="name">Path in the form state to the value being filled in.</param>
        /// <param name="condition">Delegate to test form state to see if step is active.n</param>
        /// <param name="validate">Delegate to validate the field value.</param>
        /// <remarks>
        /// This step will use reflection to construct everything needed for a dialog from a combination
        /// of the <see cref="DescribeAttribute"/>, <see cref="TermsAttribute"/>, <see cref="PromptAttribute"/>, <see cref="OptionalAttribute"/>
        /// <see cref="NumericAttribute"/> and <see cref="TemplateAttribute"/> annotations that are supplied by default or you
        /// override.
        /// </remarks>
        /// <returns>This form.</returns>
        IFormBuilder<T> Field(string name, ConditionalDelegate<T> condition = null, ValidateDelegate<T> validate = null);

        /// <summary>
        /// Define a step for filling in a particular value in the form state.
        /// </summary>
        /// <param name="name">Path in the form state to the value being filled in.</param>
        /// <param name="prompt">Simple \ref patterns to describe prompt for field.</param>
        /// <param name="condition">Delegate to test form state to see if step is active.n</param>
        /// <param name="validate">Delegate to validate the field value.</param>
        /// <returns>This form.</returns>
        /// <remarks>
        /// This step will use reflection to construct everything needed for a dialog from a combination
        /// of the <see cref="DescribeAttribute"/>, <see cref="TermsAttribute"/>, <see cref="PromptAttribute"/>, <see cref="OptionalAttribute"/>
        /// <see cref="NumericAttribute"/> and <see cref="TemplateAttribute"/> annotations that are supplied by default or you
        /// override.
        /// </remarks>
        IFormBuilder<T> Field(string name, string prompt, ConditionalDelegate<T> condition = null, ValidateDelegate<T> validate = null);

        /// <summary>
        /// Define a step for filling in a particular value in the form state.
        /// </summary>
        /// <param name="name">Path in the form state to the value being filled in.</param>
        /// <param name="prompt">Prompt pattern with more formatting control to describe prompt for field.</param>
        /// <param name="condition">Delegate to test form state to see if step is active.n</param>
        /// <param name="validate">Delegate to validate the field value.</param>
        /// <returns>This form.</returns>
        /// <remarks>
        /// This step will use reflection to construct everything needed for a dialog from a combination
        /// of the <see cref="DescribeAttribute"/>, <see cref="TermsAttribute"/>, <see cref="PromptAttribute"/>, <see cref="OptionalAttribute"/>
        /// <see cref="NumericAttribute"/> and <see cref="TemplateAttribute"/> annotations that are supplied by default or you
        /// override.
        /// </remarks>
        IFormBuilder<T> Field(string name, PromptAttribute prompt, ConditionalDelegate<T> condition = null, ValidateDelegate<T> validate = null);

        /// <summary>
        /// Derfine a field step by supplying your own field definition.
        /// </summary>
        /// <param name="field">Field definition to use.</param>
        /// <returns>This form.</returns>
        /// <remarks>
        /// You can provide your own implementation of <see cref="IField{T}"/> or you can 
        /// use the <see cref="Field{T}"/> class to provide fluent values or the <see cref="FieldReflector{T}"/>
        /// to use reflection to provide a base set of values that can be override.  It might 
        /// also make sense to derive from those classes and override the methods you need to 
        /// change.
        /// </remarks>
        IFormBuilder<T> Field(IField<T> field);

        /// <summary>
        /// Add all fields not already added to the form.
        /// </summary>
        /// <param name="exclude">Fields not to include.</param>
        /// <returns>This form.</returns>
        /// <remarks>
        /// This will add all fields defined in your forifm state that have not already been
        /// added if the fields are supported.
        /// </remarks>
        IFormBuilder<T> AddRemainingFields(IEnumerable<string> exclude = null);

        /// <summary>
        /// Add a confirmation step.
        /// </summary>
        /// <param name="prompt">Prompt to use for confirmation.</param>
        /// <param name="condition">Delegate to test if confirmation applies to the current form state.</param>
        /// <param name="dependencies">What fields this confirmation depends on.</param>
        /// <returns>This form.</returns>
        /// <remarks>
        /// If prompt is not supplied the \ref patterns element {*} will be used to confirm.
        /// Dependencies will by default be all active steps defined before this confirmation.
        /// </remarks>
        IFormBuilder<T> Confirm(string prompt = null, ConditionalDelegate<T> condition = null, IEnumerable<string> dependencies = null);

        /// <summary>
        /// Add a confirmation step.
        /// </summary>
        /// <param name="prompt">Prompt to use for confirmation.</param>
        /// <param name="condition">Delegate to test if confirmation applies to the current form state.</param>
        /// <param name="dependencies">What fields this confirmation depends on.</param>
        /// <returns>This form.</returns>
        /// <remarks>
        /// Dependencies will by default be all active steps defined before this confirmation.
        /// </remarks>
        IFormBuilder<T> Confirm(PromptAttribute prompt, ConditionalDelegate<T> condition = null, IEnumerable<string> dependencies = null);

        /// <summary>
        /// Delegate to call when form is completed.
        /// </summary>
        /// <param name="callback">Delegate to call on completion.</param>
        /// <returns>This form.</returns>
        /// <remarks>
        /// This should only be used for side effects such as calling your service with
        /// the form state results.  In any case the completed form state will be passed
        /// to the parent dialog.
        /// </remarks>
        IFormBuilder<T> OnCompletionAsync(CompletionDelegate<T> callback);
    }

    /// <summary>
    /// Default values for the form.
    /// </summary>
    /// <remarks>
    /// These defaults can all be overriden when you create a form and before you add steps.
    /// </remarks>
    public class FormConfiguration
    {

        /// <summary>
        /// Default prompt and template format settings.
        /// </summary>
        /// <remarks>
        /// When you specify a <see cref="PromptAttribute"/> or <see cref="TemplateAttribute"/>, any format 
        /// value you do not specify will come from this default.
        /// </remarks>
        public PromptAttribute DefaultPrompt = new PromptAttribute("")
        {
            AllowDefault = BoolDefault.True,
            ChoiceCase = CaseNormalization.None,
            ChoiceFormat = "{0}. {1}",
            ChoiceLastSeparator = ", or ",
            ChoiceParens = BoolDefault.True,
            ChoiceSeparator = ", ",
            ChoiceStyle = ChoiceStyleOptions.Auto,
            FieldCase = CaseNormalization.Lower,
            Feedback = FeedbackOptions.Auto,
            LastSeparator = ", and ",
            Separator = ", ",
            ValueCase = CaseNormalization.InitialUpper
        };

        /// <summary>
        /// Enumeration of strings for interpreting a user response as setting an optional field to be unspecified.
        /// </summary>
        /// <remarks>
        /// The first string is also used to describe not having a preference for an optional field.
        /// </remarks>
        public string[] NoPreference = new string[] { "No Preference", "no", "none", "I don'?t care" };

        /// <summary>
        /// Enumeration of strings for interpreting a user response as asking for the current value.
        /// </summary>
        /// <remarks>
        /// The first value is also used to describe the option of keeping the current value.
        /// </remarks>
        public string[] CurrentChoice = new string[] { "Current Choice", "current" };

        /// <summary>
        /// Enumeration of values for a "yes" response for boolean fields or confirmations.
        /// </summary>
        public string[] Yes = new string[] { "Yes", "yes", "y", "sure", "ok" };

        /// <summary>
        /// Enumeration of values for a "no" response for boolean fields or confirmations.
        /// </summary>
        public string[] No = new string[] { "No", "n" };

        /// <summary>
        /// Default templates to use if not override on the class or field level.
        /// </summary>
        public List<TemplateAttribute> Templates = new List<TemplateAttribute>
        {
            new TemplateAttribute(TemplateUsage.Bool, "Would you like a {&}? {||}"),
            // {0} is current choice, {1} is no preference
            new TemplateAttribute(TemplateUsage.BoolHelp, "Please enter 'yes' or 'no'{?, {0}}."),

            // {0} is term being clarified
            new TemplateAttribute(TemplateUsage.Clarify, "By \"{0}\" {&} did you mean {||}"),

            new TemplateAttribute(TemplateUsage.CurrentChoice, "(current choice: {})"),

            new TemplateAttribute(TemplateUsage.DateTime, "Please enter a date and time for {&} {||}"),
            // {0} is current choice, {1} is no preference
            // new TemplateAttribute(TemplateUsage.DateTimeHelp, "Please enter a date or time expression like 'Monday' or 'July 3rd'{?, {0}}{?, {1}}."),
            new TemplateAttribute(TemplateUsage.DateTimeHelp, "Please enter a date or time expression {?, {0}}{?, {1}}."),

            // {0} is min and {1} is max.
            new TemplateAttribute(TemplateUsage.Double, "Please enter a number {?between {0:F1} and {1:F1}} for {&} {||}") { ChoiceFormat = "{1}" },
            // {0} is current choice, {1} is no preference
            // {2} is min and {3} is max
            new TemplateAttribute(TemplateUsage.DoubleHelp, "Please enter a number{? between {2:F1} and {3:F1}}{?, {0}}{?, {1}}."),

            // {0} is min, {1} is max and {2} are enumerated descriptions
            new TemplateAttribute(TemplateUsage.EnumManyNumberHelp, "You can enter one or more numbers {0}-{1} or words from the descriptions. ({2})"),
            new TemplateAttribute(TemplateUsage.EnumOneNumberHelp, "You can enter a number {0}-{1} or words from the descriptions. ({2})"),

            // {2} are the words people can type
            new TemplateAttribute(TemplateUsage.EnumManyWordHelp, "You can enter in one or more selections from the descriptions. ({2})"),
            new TemplateAttribute(TemplateUsage.EnumOneWordHelp, "You can enter in any words from the descriptions. ({2})"),

            new TemplateAttribute(TemplateUsage.EnumSelectOne, "Please select a {&} {||}"),
            new TemplateAttribute(TemplateUsage.EnumSelectMany, "Please select one or more {&} {||}"),

            // {0} is the not understood term
            new TemplateAttribute(TemplateUsage.Feedback, "For {&} I understood {}. {?\"{0}\" is not an option.}"),

            // For {0} is recognizer help and {1} is command help.
            new TemplateAttribute(TemplateUsage.Help, "You are filling in the {&} field.  Possible responses:\n{0}\n{1}"),
            new TemplateAttribute(TemplateUsage.HelpClarify, "You are clarifying a {&} value.  Possible responses:\n{0}\n{1}"),
            new TemplateAttribute(TemplateUsage.HelpConfirm, "Please answer the question.  Possible responses:\n{0}\n{1}"),
            new TemplateAttribute(TemplateUsage.HelpNavigation, "Choose what field to change.  Possible responses:\n{0}\n{1}"),

            // {0} is min and {1} is max if present
            new TemplateAttribute(TemplateUsage.Integer, "Please enter a number{? between {0} and {1}} for {&} {||}") { ChoiceFormat = "{1}" },
            // {0} is current choice, {1} is no preference
            // {2} is min and {3} is max
            new TemplateAttribute(TemplateUsage.IntegerHelp, "You can enter a number{? between {2} and {3}}{?, {0}}{?, {1}}."),

            new TemplateAttribute(TemplateUsage.Navigation, "What do you want to change? {||}") { FieldCase = CaseNormalization.None },
            // {0} is list of field names.
            new TemplateAttribute(TemplateUsage.NavigationCommandHelp, "You can switch to another field by entering its name. ({0})."),
            new TemplateAttribute(TemplateUsage.NavigationFormat, "{&}({})") {FieldCase = CaseNormalization.None },
            // {0} is min, {1} is max
            new TemplateAttribute(TemplateUsage.NavigationHelp, "Choose {?a number from {0}-{1}, or} a field name."),

            new TemplateAttribute(TemplateUsage.NoPreference, "No Preference"),

            // {0} is the term that is not understood
            new TemplateAttribute(TemplateUsage.NotUnderstood, @"""{0}"" is not a {&} option."),

            new TemplateAttribute(TemplateUsage.StatusFormat, "{&}: {}") {FieldCase = CaseNormalization.None },

            new TemplateAttribute(TemplateUsage.String, "Please enter {&} {||}") { ChoiceFormat = "{1}" },
            // {0} is current choice, {1} is no preference
			new TemplateAttribute(TemplateUsage.StringHelp, "You can enter anything (use \"'s to force string){?, {0}}{?, {1}}."),

            new TemplateAttribute(TemplateUsage.Unspecified, "Unspecified")
        };

        /// <summary>
        /// Definitions of the built-in commands.
        /// </summary>
        public Dictionary<FormCommand, CommandDescription> Commands = new Dictionary<FormCommand, CommandDescription>()
        {
            {FormCommand.Backup, new CommandDescription("Backup", new string[] {"backup", "go back", "back" },
                "Back: Go back to the previous question.") },
            {FormCommand.Help, new CommandDescription("Help", new string[] { "help", "choices", @"\?" },
                "Help: Show the kinds of responses you can enter.") },
            {FormCommand.Quit, new CommandDescription("Quit", new string[] { "quit", "stop", "finish", "goodbye", "good bye"},
                "Quit: Quit the form without completing it.") },
            {FormCommand.Reset, new CommandDescription("Start over", new string[] { "start over", "reset", "clear" },
                "Reset: Start over filling in the form.  (With defaults from your previous entries.)" ) },
            {FormCommand.Status, new CommandDescription("status", new string[] {"status", "progress", "so far" },
                "Status: Show your progress in filling in the form so far.") }
        };

        /// <summary>
        /// Look up a particular template.
        /// </summary>
        /// <param name="usage">Desired template.</param>
        /// <returns>Matching template.</returns>
        public TemplateAttribute Template(TemplateUsage usage)
        {
            TemplateAttribute result = null;
            foreach (var template in Templates)
            {
                if (template.Usage == usage)
                {
                    result = template;
                    break;
                }
            }
            Debug.Assert(result != null);
            return result;
        }
    };
}
