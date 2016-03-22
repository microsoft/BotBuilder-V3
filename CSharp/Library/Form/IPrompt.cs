using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Form.Advanced
{

    /// <summary>
    /// Interface for a prompt and its associated recognizer.
    /// </summary>
    /// <typeparam name="T">Form state.</typeparam>
    /// <remarks>
    /// This interface allows taking a \ref patterns expression and making it into a string with the template parts filled in.
    /// </remarks>
    public interface IPrompt<T>
        where T : class, new()
    {
        /// <summary>
        /// Description of the prompt and how to generate it.
        /// </summary>
        /// <returns>Attribute describing how to generate prompt.</returns>
        TemplateBase Annotation();

        /// <summary>
        /// Return string to send to user.
        /// </summary>
        /// <param name="state">Current form state.</param>
        /// <param name="path">Current field being processed.</param>
        /// <param name="args">Optional arguments.</param>
        /// <returns>Message to user.</returns>
        string Prompt(T state, string path, params object[] args);

        /// <summary>
        /// Associated recognizer if any.
        /// </summary>
        /// <returns>Recognizer for matching user input.</returns>
        IRecognizer<T> Recognizer();
    }

    public sealed class Prompter<T> : IPrompt<T>
        where T : class, new()
    {
        /// <summary>
        /// Construct a prompter.
        /// </summary>
        /// <param name="annotation">Annotation describing the \ref patterns and formatting for prompt.</param>
        /// <param name="form">Current form.</param>
        /// <param name="recognizer">Recognizer if any.</param>
        public Prompter(TemplateBase annotation, IForm<T> form, IRecognizer<T> recognizer)
        {
            annotation.ApplyDefaults(form.Configuration().DefaultPrompt);
            _annotation = annotation;
            _form = form;
            _fields = form.Fields();
            _recognizer = recognizer;
        }

        public TemplateBase Annotation()
        {
            return _annotation;
        }

        public string Prompt(T state, string pathName, params object[] args)
        {
            string currentChoice = null;
            string noValue = null;
            if (pathName != "")
            {
                var field = _fields.Field(pathName);
                currentChoice = field.Template(TemplateUsage.CurrentChoice).Pattern();
                if (field.Optional())
                {
                    noValue = field.Template(TemplateUsage.NoPreference).Pattern();
                }
                else
                {
                    noValue = field.Template(TemplateUsage.Unspecified).Pattern();
                }
            }
            var response = ExpandTemplate(_annotation.Pattern(), currentChoice, noValue, state, pathName, args);
            return (response == null ? "" : _spacesPunc.Replace(_spaces.Replace(Language.ANormalization(response), "$1 "), "$1"));
        }

        private string ExpandTemplate(string template, string currentChoice, string noValue, T state, string pathName, object[] args)
        {
            bool foundUnspecified = false;
            int last = 0;
            int numeric;
            var response = new StringBuilder();
            var field = _fields.Field(pathName);
            foreach (Match match in _args.Matches(template))
            {
                var expr = match.Groups[1].Value.Trim();
                var substitute = "";
                if (expr.StartsWith("&"))
                {
                    var spec = expr.Substring(1).Split(':');
                    var name = spec[0];
                    if (name == "")
                    {
                        // Use default pathname
                        name = pathName;
                    }
                    var pathField = _fields.Field(name);
                    substitute = Normalize(pathField == null ? pathName : pathField.Description(), _annotation.FieldCase);
                }
                else if (expr == "||")
                {
                    var builder = new StringBuilder();
                    var defaultValue = field.GetValue(state);
                    var values = _recognizer.ValueDescriptions();
                    if (_annotation.AllowDefault != BoolDefault.False)
                    {
                        if (!field.Optional())
                        {
                            if (!field.IsUnknown(state))
                            {
                                builder.Append(ExpandTemplate(currentChoice, null, noValue, state, pathName, args));
                                builder.Append(' ');
                            }
                        }
                        else
                        {
                            if (field.IsUnknown(state))
                            {
                                builder.Append(ExpandTemplate(currentChoice, null, noValue, state, pathName, args));
                                builder.Append(' ');
                            }
                            else
                            {
                                builder.Append(ExpandTemplate(currentChoice, null, noValue, state, pathName, args));
                                builder.Append(' ');
                                values = values.Concat(new string[] { noValue });
                            }
                        }
                    }
                    if (values.Count() > 0)
                    {
                        if ((_annotation.ChoiceStyle == ChoiceStyleOptions.Auto && values.Count() < 4)
                            || (_annotation.ChoiceStyle == ChoiceStyleOptions.Inline))
                        {
                            // Inline choices
                            bool first = true;
                            builder.Append('(');
                            var i = 1;
                            foreach (var value in values)
                            {
                                if (first)
                                {
                                    first = false;
                                }
                                else
                                {
                                    builder.Append(", ");
                                }
                                builder.AppendFormat(_annotation.ChoiceFormat, i, value);
                                ++i;
                            }
                            builder.Append(')');
                        }
                        else
                        {
                            // Seperate line choices
                            var i = 1;
                            foreach (var value in values)
                            {
                                builder.Append("\n  ");
                                if (_annotation.AllowNumbers != BoolDefault.True)
                                {
                                    builder.Append("* ");
                                }
                                builder.AppendFormat(_annotation.ChoiceFormat, i, value);
                                ++i;
                            }
                        }
                    }
                    substitute = builder.ToString();
                }
                else if (expr.StartsWith("*"))
                {
                    // Status display of active results
                    var filled = expr.ToLower().Trim().EndsWith("filled");
                    var builder = new StringBuilder();
                    var format = new Prompter<T>(Template(field, TemplateUsage.StatusFormat), _form, null);
                    if (match.Index > 0)
                    {
                        builder.Append("\n");
                    }
                    foreach (var entry in (from step in _fields where (!filled || !step.IsUnknown(state)) && step.Role() == FieldRole.Value && step.Active(state) select step))
                    {
                        builder.Append("* ").AppendLine(format.Prompt(state, entry.Name()));
                    }
                    substitute = builder.ToString();
                }
                else if (expr.StartsWith("[") && expr.EndsWith("]"))
                {
                    // Generate a list from multiple fields
                    var paths = expr.Substring(1, expr.Length - 2).Split(' ');
                    var values = new List<Tuple<IField<T>, object>>();
                    foreach (var spec in paths)
                    {
                        if (!spec.StartsWith("{") || !spec.EndsWith("}"))
                        {
                            throw new ArgumentException("Only {<field>} references are allowed in lists.");
                        }
                        var name = spec.Substring(1, spec.Length - 2).Trim();
                        var eltDesc = _fields.Field(name);
                        if (!eltDesc.IsUnknown(state))
                        {
                            var value = eltDesc.GetValue(state);
                            if (value.GetType() != typeof(string) && value.GetType().IsIEnumerable())
                            {
                                var eltValues = (value as System.Collections.IEnumerable);
                                foreach (var elt in eltValues)
                                {
                                    values.Add(Tuple.Create(eltDesc, elt));
                                }
                            }
                            else
                            {
                                values.Add(Tuple.Create(eltDesc, eltDesc.GetValue(state)));
                            }
                        }
                    }
                    if (values.Count() > 0)
                    {
                        var elements = (from elt in values select Normalize(ValueDescription(elt.Item1, elt.Item2), _annotation.ValueCase)).ToArray();
                        substitute = Language.BuildList(elements, _annotation.Separator, _annotation.LastSeparator);
                    }
                }
                else if (expr.StartsWith("?"))
                {
                    // Conditional template
                    var subValue = ExpandTemplate(expr.Substring(1), currentChoice, null, state, pathName, args);
                    if (subValue == null)
                    {
                        substitute = "";
                    }
                    else
                    {
                        substitute = subValue;
                    }
                }
                else if (TryParseFormat(expr, out numeric))
                {
                    // Process ad hoc arg
                    if (numeric < args.Length && args[numeric] != null)
                    {
                        substitute = string.Format("{" + expr + "}", args);
                    }
                    else
                    {
                        foundUnspecified = true;
                        break;
                    }
                }
                else
                {
                    var name = expr;
                    if (name == "") name = pathName;
                    var pathDesc = _fields.Field(name);
                    if (pathDesc.IsUnknown(state))
                    {
                        if (noValue == null)
                        {
                            foundUnspecified = true;
                            break;
                        }
                        substitute = noValue;
                    }
                    else
                    {
                        var value = pathDesc.GetValue(state);
                        if (value.GetType() != typeof(string) && value.GetType().IsIEnumerable())
                        {
                            var values = (value as System.Collections.IEnumerable);
                            substitute = Language.BuildList(from elt in values.Cast<object>() select Normalize(ValueDescription(pathDesc, elt), _annotation.ValueCase),
                                _annotation.Separator, _annotation.LastSeparator);
                        }
                        else
                        {
                            substitute = ValueDescription(pathDesc, value);
                        }
                    }
                }
                response.Append(template.Substring(last, match.Index - last)).Append(substitute);
                last = match.Index + match.Length;
            }
            return (foundUnspecified ? null : response.Append(template.Substring(last, template.Length - last)).ToString());
        }

        private bool TryParseFormat(string format, out int number)
        {
            var args = format.Split(':');
            return int.TryParse(args[0], out number);
        }

        private string ValueDescription(IField<T> field, object value)
        {
            return field.Prompt().Recognizer().ValueDescription(value);
        }

        public IRecognizer<T> Recognizer()
        {
            return _recognizer;
        }

        private string Normalize(string value, CaseNormalization normalization)
        {
            switch (normalization)
            {
                case CaseNormalization.InitialUpper:
                    string.Join(" ", (from word in Language.WordBreak(value)
                                      select char.ToUpper(word[0]) + word.Substring(1).ToLower()));
                    break;
                case CaseNormalization.Lower: value = value.ToLower(); break;
                case CaseNormalization.Upper: value = value.ToUpper(); break;
            }
            return value;
        }

        private Template Template(IField<T> field, TemplateUsage usage)
        {
            return field == null
                ? _form.Configuration().Template(usage)
                : field.Template(usage);
        }

        private static Regex _args = new Regex(@"{((?>[^{}]+|{(?<number>)|}(?<-number>))*(?(number)(?!)))}", RegexOptions.Compiled);
        private static Regex _spaces = new Regex(@"(\S)( {2,})", RegexOptions.Compiled);
        private static Regex _spacesPunc = new Regex(@"(?:\s+)(\.|\?)", RegexOptions.Compiled);
        private IForm<T> _form;
        private IFields<T> _fields;
        private TemplateBase _annotation;
        private IRecognizer<T> _recognizer;
    }
}
