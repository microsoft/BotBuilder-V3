using Microsoft.Bot.Builder.Form.Advanced;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Bot.Builder.Form
{
    /// <summary>
    /// A delegate for testing a form state to see if a particular step is active.
    /// </summary>
    /// <typeparam name="T">Form state type.</typeparam>
    /// <param name="state">Form state to test.</param>
    /// <returns>True if step is active given the current form state.</returns>
    public delegate bool ConditionalDelegate<T>(T state);

    /// <summary>
    /// A delegate for validating a particular response to a prompt.
    /// </summary>
    /// <typeparam name="T">Form state type.</typeparam>
    /// <param name="state">Form state to test.</param>
    /// <param name="value">Response value to validate.</param>
    /// <returns>Null if value is valid otherwise feedback on what is wrong.</returns>
    public delegate string ValidateDelegate<T>(T state, object value);

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
    public delegate void CompletionDelegate<T>(IDialogContext context, T state);

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
    public interface IForm<T> : IDialog
        where T : class, new()
    {
        /// <summary>
        /// The model for the form.
        /// </summary>
        IFormModel<T> Model { get; }
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
        /// When you specify a <see cref="Prompt"/> or <see cref="Template"/>, any format 
        /// value you do not specify will come from this default.
        /// </remarks>
        public Prompt DefaultPrompt = new Prompt("")
        {
            AllowDefault = BoolDefault.True,
            AllowNumbers = BoolDefault.True,
            ChoiceStyle = ChoiceStyleOptions.Auto,
            FieldCase = CaseNormalization.Lower,
            Feedback = FeedbackOptions.Auto,
            ChoiceFormat = "{0}. {1}",
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
        public List<Template> Templates = new List<Template>
        {
            new Template(TemplateUsage.Bool, "Would you like a {&}? {||}"),
            new Template(TemplateUsage.BoolHelp, "Please enter 'yes' or 'no'{?, {0}}."),

            // {0} is term being clarified
            new Template(TemplateUsage.Clarify, "By \"{0}\" {&} did you mean {||}"),
            new Template(TemplateUsage.CurrentChoice, "(current choice: {})"),

            new Template(TemplateUsage.DateTime, "Please enter a date and time for {&} {||}"),
            // {0} is current choice, {1} is no preference
            new Template(TemplateUsage.DateTimeHelp, "Please enter a date or time expression like 'Monday' or 'July 3rd'{?, {0}}{?, {1}}."),

            new Template(TemplateUsage.Double, "Please enter a number for {&} {||}") { AllowNumbers = BoolDefault.False, ChoiceFormat = "{1}" },

            // {0} is current choice, {1} is no preference
            // {2} is min and {3} is max
            new Template(TemplateUsage.DoubleHelp, "Please enter a number{? between {2:F1} and {3:F1}}{?, {0}}{?, {1}}."),

            // {0} is min, {1} is max and {2} are enumerated descriptions
            new Template(TemplateUsage.EnumOneNumberHelp, "You can enter a number {0}-{1} or words from the descriptions. ({2})"),
            new Template(TemplateUsage.EnumManyNumberHelp, "You can enter one or more numbers {0}-{1} or words from the descriptions. ({2})"),

            // {0} are the words people can type
            new Template(TemplateUsage.EnumOneWordHelp, "You can enter in any words from the descriptions. ({2})"),
            new Template(TemplateUsage.EnumManyWordHelp, "You can enter in one or more selections from the descriptions. ({2})"),

            new Template(TemplateUsage.EnumSelectOne, "Please select a {&} {||}"),
            new Template(TemplateUsage.EnumSelectMany, "Please select one or more {&} {||}"),

            // {0} is the not understood term
            new Template(TemplateUsage.Feedback, "For {&} I understood {}. {?\"{0}\" is not an option.}"),

            // For {0} is recognizer help and {1} is command help.
            new Template(TemplateUsage.Help, "You are filling in the {&} field.  Possible responses:\n{0}\n{1}"),
            new Template(TemplateUsage.HelpConfirm, "Please answer the question.  Possible responses:\n{0}\n{1}"),
            new Template(TemplateUsage.HelpClarify, "You are clarifying a {&} value.  Possible responses:\n{0}\n{1}"),
            new Template(TemplateUsage.HelpNavigation, "Choose what field to change.  Possible responses:\n{0}\n{1}"),

            // {0} is min and {1} is max if present
            new Template(TemplateUsage.Integer, "Please enter a number{? between {0} and {1}} for {&} {||}") { AllowNumbers = BoolDefault.False, ChoiceFormat = "{1}" },
            // {0} is current choice, {1} is no preference
            // {2} is min and {3} is max
            new Template(TemplateUsage.IntegerHelp, "You can enter a number{? between {2} and {3}}{?, {0}}{?, {1}}."),

            new Template(TemplateUsage.Navigation, "What do you want to change? {||}") { FieldCase = CaseNormalization.None },
            // {0} is list of field names.
            new Template(TemplateUsage.NavigationCommandHelp, "You can switch to another field by entering its name. ({0})."),
            new Template(TemplateUsage.NavigationFormat, "{&}({})") {FieldCase = CaseNormalization.None },
            new Template(TemplateUsage.NavigationHelp, "Choose {?a number from {0}-{1}, or} a field name."),

            new Template(TemplateUsage.NoPreference, "No Preference"),

            // {0} is the term that is not understood
            new Template(TemplateUsage.NotUnderstood, @"""{0}"" is not a {&} option."),

            new Template(TemplateUsage.StatusFormat, "{&}: {}") {FieldCase = CaseNormalization.None },

            new Template(TemplateUsage.String, "Please enter {&} {||}") { AllowNumbers = BoolDefault.False, ChoiceFormat = "{1}" },
 
            // {0} is current choice, {1} is no preference
			new Template(TemplateUsage.StringHelp, "You can enter anything{?, {0}}{?, {1}}."),

            new Template(TemplateUsage.Unspecified, "Unspecified")
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
                "Reset: Start over filling in the form.  (With defaults of your previous entries.)" ) },
            {FormCommand.Status, new CommandDescription("status", new string[] {"status", "progress", "so far" },
                "Status: Show your progress in filling in the form so far.") }
        };

        /// <summary>
        /// Look up a particular template.
        /// </summary>
        /// <param name="usage">Desired template.</param>
        /// <returns>Matching template.</returns>
        public Template Template(TemplateUsage usage)
        {
            Template result = null;
            foreach(var template in Templates)
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

