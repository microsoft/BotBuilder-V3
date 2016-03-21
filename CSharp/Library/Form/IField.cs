using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Form.Advanced
{
    public interface IFieldState<T>
        where T : class, new()
    {
        /// <summary>
        /// Get a value from state.
        /// </summary>
        /// <param name="state">Object with value.</param>
        /// <returns>Current value found in state.</returns>
        object GetValue(T state);

        /// <summary>
        /// Set a value in T.
        /// </summary>
        /// <param name="state">Object to change.</param>
        /// <param name="value">New value.</param>
        void SetValue(T state, object value);

        /// <summary>
        /// Test to see if a value in T is known or not.
        /// </summary>
        /// <param name="state">Object with value to check.</param>
        /// <returns>True is value is unknown.</returns>
        bool IsUnknown(T state);

        /// <summary>
        /// Set a value in T to unknown.
        /// </summary>
        /// <param name="state">Object to change.</param>
        void SetUnknown(T state);

        /// <summary>
        /// Test to see if field is optional which means that no value is legal.
        /// </summary>
        /// <returns>True if field is optional.</returns>
        bool Optional();

        /// <summary>
        /// Test to see if field allows setting null as value.
        /// </summary>
        /// <returns>True if field is nullable.</returns>
        bool IsNullable();

        /// <summary>
        /// Limits of numeric values.
        /// </summary>
        /// <param name="min">Minimum possible value.</param>
        /// <param name="max">Maximum possible value.</param>
        /// <returns>True if limits limit the underlying data type.</returns>
        bool Limits(out double min, out double max);

        /// <summary>
        /// Returns the other fields this one depends on.
        /// </summary>
        /// <returns>List of field names this one depends on.</returns>
        IEnumerable<string> Dependencies();
    }

    public enum FieldRole { Value, Confirm };

    public interface IFieldDescription
    {
        /// <summary>
        /// Type of field.
        /// </summary>
        /// <returns>Type of field.</returns>
        FieldRole Role();

        /// <summary>
        /// Decription of the field itself.
        /// </summary>
        /// <returns>Field description.</returns>
        string Description();

        /// <summary>
        /// Terms for matching this field.
        /// </summary>
        /// <returns>List of term regexs for matching the field name.</returns>
        IEnumerable<string> Terms();

        /// <summary>
        /// Return the string describing a specific value.
        /// </summary>
        /// <param name="value">Value being described.</param>
        /// <returns>String describing value.</returns>
        string ValueDescription(object value);

        /// <summary>
        /// Return all possible value descriptions in order to support enumeration.
        /// </summary>
        /// <returns>All possible value descriptions.</returns>
        IEnumerable<string> ValueDescriptions();

        /// <summary>
        /// Given an object return terms that can be used in a dialog to match the object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IEnumerable<string> Terms(object value);

        /// <summary>
        /// All possible values or null if it is a data type like number.
        /// </summary>
        /// <returns>All possible values.</returns>
        IEnumerable<object> Values();

        /// <summary>
        /// Are multiple matches allowed.
        /// </summary>
        /// <returns>True if more than one value is allowed.</returns>
        bool AllowsMultiple();

        /// <summary>
        /// Allow the default value as an option.
        /// </summary>
        /// <returns>True if default values are allowed.</returns>
        bool AllowDefault();

        /// <summary>
        /// Allow numbers to be matched.
        /// </summary>
        /// <returns>True if numbers are allowed as input.</returns>
        bool AllowNumbers();
    }

    /// <summary>
    /// Direction for next step.
    /// </summary>
    public enum StepDirection
    {
        Complete,

        /// <summary>
        /// Move to a named step.  If there is more than one name, the user will be asked to choose.
        /// </summary>
        Named,

        /// <summary>
        /// Move to the next step that is Active() and ready.
        /// </summary>
        Next,

        /// <summary>
        /// Move to the previously executed step.
        /// </summary>
        Previous,

        /// <summary>
        /// Quit the form.
        /// </summary>
        Quit,

        /// <summary>
        /// Reset the form to start over.
        /// </summary>
        Reset
    };

    /// <summary>
    /// Next step to take.
    /// </summary>
    [Serializable]
    public class NextStep
    {
        public NextStep()
        {
            Direction = StepDirection.Next;
        }

        public NextStep(StepDirection direction)
        {
            Direction = direction;
        }

        public NextStep(IEnumerable<string> names)
        {
            Direction = StepDirection.Named;
            Names = names.ToArray();
        }

        /// <summary>
        /// Direction for next step.
        /// </summary>
        public StepDirection Direction;

        /// <summary>
        /// If this is a named step, one or more named steps to move to.  If there are more than one, the user will choose.
        /// </summary>
        public string[] Names;
    }

    public interface IFieldPrompt<T>
        where T : class, new()
    {
        /// <summary>
        /// Test to see if field is currently active based on the current state.
        /// </summary>
        /// <returns>True if field is active.</returns>
        bool Active(T state);

        /// <summary>
        /// Return a template for building a prompt.
        /// </summary>
        /// <param name="usage">Kind of template we are looking for.</param>
        /// <returns>NULL if no template, otherwise a template annotation.</returns>
        Template Template(TemplateUsage usage);

        /// <summary>
        /// Return the prompt associated with a field.
        /// </summary>
        /// <returns>A prompt and recognizer packaged together.</returns>
        IPrompt<T> Prompt();

        /// <summary>
        /// Validate value to be set on state and return feedback if not valid.
        /// </summary>
        /// <param name="state">State before setting value.</param>
        /// <param name="value">Value to be set in field.</param>
        /// <returns>Null if OK, otherwise feedback on what should change.</returns>
        string Validate(T state, object value);

        /// <summary>
        /// Return the help description for this field.
        /// </summary>
        /// <returns></returns>
        IPrompt<T> Help();

        /// <summary>
        /// Next step to execute.
        /// </summary>
        /// <param name="value">Value in response to prompt.</param>
        /// <param name="state">Current state.</param>
        /// <returns>Next step to execute.</returns>
        NextStep Next(object value, T state);
    }

    public interface IField<T> : IFieldState<T>, IFieldDescription, IFieldPrompt<T>
        where T : class, new()
    {
        string Name();

        IForm<T> Form();

        void SetForm(IForm<T> form);
    }

    public interface IFields<T> : IEnumerable<IField<T>>
        where T : class, new()
    {
        IField<T> Field(string name);
    }
}
