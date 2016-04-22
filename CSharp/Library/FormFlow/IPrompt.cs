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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.FormFlow.Advanced
{

    /// <summary>
    /// Interface for a prompt and its associated recognizer.
    /// </summary>
    /// <typeparam name="T">Form state.</typeparam>
    /// <remarks>
    /// This interface allows taking a \ref patterns expression and making it into a string with the template parts filled in.
    /// </remarks>
    public interface IPrompt<T>
    {
        /// <summary>
        /// Description of the prompt and how to generate it.
        /// </summary>
        /// <returns>Attribute describing how to generate prompt.</returns>
        TemplateBaseAttribute Annotation { get; }

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
        IRecognize<T> Recognizer { get; }
    }

    #region Documentation
    /// <summary>   A prompt and recognizer packaged together. </summary>
    /// <typeparam name="T">    UNderlying form type. </typeparam>
    #endregion
    public sealed class Prompter<T> : IPrompt<T>
    {
        /// <summary>
        /// Construct a prompter.
        /// </summary>
        /// <param name="annotation">Annotation describing the \ref patterns and formatting for prompt.</param>
        /// <param name="form">Current form.</param>
        /// <param name="recognizer">Recognizer if any.</param>
        public Prompter(TemplateBaseAttribute annotation, IForm<T> form, IRecognize<T> recognizer)
        {
            annotation.ApplyDefaults(form.Configuration.DefaultPrompt);
            _annotation = annotation;
            _form = form;
            _recognizer = recognizer;
        }

        public TemplateBaseAttribute Annotation
        {
            get
            {
                return _annotation;
            }
        }

        public string Prompt(T state, string pathName, params object[] args)
        {
            string currentChoice = null;
            string noValue = null;
            if (pathName != "")
            {
                var field = _form.Fields.Field(pathName);
                currentChoice = field.Template(TemplateUsage.CurrentChoice).Pattern();
                if (field.Optional)
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

        public IRecognize<T> Recognizer
        {
            get { return _recognizer; }
        }

        #region Documentation
        /// <summary>   Validate pattern by ensuring they refer to real fields. </summary>
        /// <param name="form">     The form. </param>
        /// <param name="pattern">  Specifies the pattern. </param>
        /// <param name="pathName"> Full pathname of the field. </param>
        /// <param name="argLimit"> The number of arguments passed to the pattern. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        #endregion
        public static bool ValidatePattern(IForm<T> form, string pattern, string pathName, int argLimit = 0)
        {
            bool ok = true;
            var fields = form.Fields;
            foreach (Match match in _args.Matches(pattern))
            {
                var expr = match.Groups[1].Value.Trim();
                int numeric;
                if (expr == "||")
                {
                    ok = true;
                }
                else if (expr.StartsWith("&"))
                {
                    var name = expr.Substring(1);
                    if (name == "") name = pathName;
                    ok = (name == "" || fields.Field(name) != null);
                }
                else if (expr.StartsWith("?"))
                {
                    ok = ValidatePattern(form, expr.Substring(1), pathName, argLimit);
                }
                else if (expr.StartsWith("["))
                {
                    if (expr.EndsWith("]"))
                    {
                        ok = ValidatePattern(form, expr.Substring(1, expr.Length - 2), pathName, argLimit);
                    }
                    else
                    {
                        ok = false;
                    }
                }
                else if (expr.StartsWith("*"))
                {
                    ok = (expr == "*" || expr == "*filled");
                }
                else if (TryParseFormat(expr, out numeric))
                {
                    ok = numeric <= argLimit - 1;
                }
                else
                {
                    var formatArgs = expr.Split(':');
                    var name = formatArgs[0];
                    if (name == "") name = pathName;
                    ok = (name == "" || fields.Field(name) != null);
                }
                if (!ok)
                {
                    break;
                }
            }
            return ok;
        }

        private string ExpandTemplate(string template, string currentChoice, string noValue, T state, string pathName, object[] args)
        {
            bool foundUnspecified = false;
            int last = 0;
            int numeric;
            var response = new StringBuilder();
            var field = _form.Fields.Field(pathName);
            foreach (Match match in _args.Matches(template))
            {
                var expr = match.Groups[1].Value.Trim();
                var substitute = "";
                if (expr.StartsWith("&"))
                {
                    var name = expr.Substring(1);
                    if (name == "") name = pathName;
                    var pathField = _form.Fields.Field(name);
                    substitute = Language.Normalize(pathField == null ? pathName : pathField.FieldDescription, _annotation.FieldCase);
                }
                else if (expr == "||")
                {
                    var builder = new StringBuilder();
                    var defaultValue = field.GetValue(state);
                    var values = _recognizer.ValueDescriptions();
                    if (_annotation.AllowDefault != BoolDefault.False && field.Optional && !field.IsUnknown(state))
                    {
                        values = values.Concat(new string[] { Language.Normalize(noValue, _annotation.ChoiceCase) });
                    }
                    string current = null;
                    if (_annotation.AllowDefault != BoolDefault.False)
                    {
                        if (!field.Optional)
                        {
                            if (!field.IsUnknown(state))
                            {
                                current = ExpandTemplate(currentChoice, null, noValue, state, pathName, args);
                            }
                        }
                        else
                        {
                            current = ExpandTemplate(currentChoice, null, noValue, state, pathName, args);
                        }
                    }
                    if (values.Count() > 0)
                    {
                        if ((_annotation.ChoiceStyle == ChoiceStyleOptions.Auto && values.Count() < 4)
                            || (_annotation.ChoiceStyle == ChoiceStyleOptions.Inline))
                        {
                            // Inline choices
                            if (_annotation.ChoiceParens == BoolDefault.True) builder.Append('(');
                            var choices = new List<string>();
                            var i = 1;
                            foreach (var value in values)
                            {
                                choices.Add(string.Format(_annotation.ChoiceFormat, i, Language.Normalize(value, _annotation.ChoiceCase)));
                                ++i;
                            }
                            builder.Append(Language.BuildList(choices, _annotation.ChoiceSeparator, _annotation.ChoiceLastSeparator));
                            if (_annotation.ChoiceParens == BoolDefault.True) builder.Append(')');
                            if (current != null)
                            {
                                builder.Append(" ");
                                builder.Append(current);
                            }
                        }
                        else
                        {
                            // Separate line choices
                            if (current != null)
                            {
                                builder.Append(current);
                                builder.Append(" ");
                            }
                            var i = 1;
                            foreach (var value in values)
                            {
                                builder.Append("\n  ");
                                if (!_annotation.AllowNumbers)
                                {
                                    builder.Append("* ");
                                }
                                builder.AppendFormat(_annotation.ChoiceFormat, i, Language.Normalize(value, _annotation.ChoiceCase));
                                ++i;
                            }
                        }
                    }
                    else if (current != null)
                    {
                        builder.Append(" ");
                        builder.Append(current);
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
                    foreach (var entry in (from step in _form.Fields where (!filled || !step.IsUnknown(state)) && step.Role == FieldRole.Value && step.Active(state) select step))
                    {
                        builder.Append("* ").AppendLine(format.Prompt(state, entry.Name));
                    }
                    substitute = builder.ToString();
                }
                else if (expr.StartsWith("[") && expr.EndsWith("]"))
                {
                    // Generate a list from multiple fields
                    var paths = expr.Substring(1, expr.Length - 2).Split(' ');
                    var values = new List<Tuple<IField<T>, object, string>>();
                    foreach (var spec in paths)
                    {
                        if (!spec.StartsWith("{") || !spec.EndsWith("}"))
                        {
                            throw new ArgumentException("Only {<field>} references are allowed in lists.");
                        }
                        var formatArgs = spec.Substring(1, spec.Length - 2).Trim().Split(':');
                        var name = formatArgs[0];
                        if (name == "") name = pathName;
                        var format = (formatArgs.Length > 1 ? "0:" + formatArgs[1] : "0");
                        var eltDesc = _form.Fields.Field(name);
                        if (!eltDesc.IsUnknown(state))
                        {
                            var value = eltDesc.GetValue(state);
                            if (value.GetType() != typeof(string) && value.GetType().IsIEnumerable())
                            {
                                var eltValues = (value as System.Collections.IEnumerable);
                                foreach (var elt in eltValues)
                                {
                                    values.Add(Tuple.Create(eltDesc, elt, format));
                                }
                            }
                            else
                            {
                                values.Add(Tuple.Create(eltDesc, eltDesc.GetValue(state), format));
                            }
                        }
                    }
                    if (values.Count() > 0)
                    {
                        var elements = (from elt in values
                                        select Language.Normalize(ValueDescription(elt.Item1, elt.Item2, elt.Item3), _annotation.ValueCase)).ToArray();
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
                    var formatArgs = expr.Split(':');
                    var name = formatArgs[0];
                    if (name == "") name = pathName;
                    var pathDesc = _form.Fields.Field(name);
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
                            substitute = Language.BuildList(from elt in values.Cast<object>()
                                                            select Language.Normalize(ValueDescription(pathDesc, elt, "0"), _annotation.ValueCase),
                                _annotation.Separator, _annotation.LastSeparator);
                        }
                        else
                        {
                            var format = (formatArgs.Length > 1 ? "0:" + formatArgs[1] : "0");
                            substitute = ValueDescription(pathDesc, value, format);
                        }
                    }
                }
                response.Append(template.Substring(last, match.Index - last)).Append(substitute);
                last = match.Index + match.Length;
            }
            return (foundUnspecified ? null : response.Append(template.Substring(last, template.Length - last)).ToString());
        }

        private static bool TryParseFormat(string format, out int number)
        {
            var args = format.Split(':');
            return int.TryParse(args[0], out number);
        }

        private string ValueDescription(IField<T> field, object value, string format)
        {
            string result;
            if (format != "0")
            {
                result = string.Format("{" + format + "}", value);
            }
            else
            {
                result = field.Prompt.Recognizer.ValueDescription(value);
            }
            return result;
        }

        private TemplateAttribute Template(IField<T> field, TemplateUsage usage)
        {
            return field == null
                ? _form.Configuration.Template(usage)
                : field.Template(usage);
        }

        private static readonly Regex _args = new Regex(@"{((?>[^{}]+|{(?<number>)|}(?<-number>))*(?(number)(?!)))}", RegexOptions.Compiled);
        private static readonly Regex _spaces = new Regex(@"(\S)( {2,})", RegexOptions.Compiled);
        private static readonly Regex _spacesPunc = new Regex(@"(?:\s+)(\.|\?)", RegexOptions.Compiled);
        private IForm<T> _form;
        private TemplateBaseAttribute _annotation;
        private IRecognize<T> _recognizer;
    }
}
