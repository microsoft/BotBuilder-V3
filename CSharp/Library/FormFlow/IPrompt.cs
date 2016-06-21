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

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Resource;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
        /// Return prompt to send to user.
        /// </summary>
        /// <param name="state">Current form state.</param>
        /// <param name="path">Current field being processed.</param>
        /// <param name="args">Optional arguments.</param>
        /// <returns>Message to user.</returns>
        FormPrompt Prompt(T state, string path, params object[] args);

        /// <summary>
        /// Associated recognizer if any.
        /// </summary>
        /// <returns>Recognizer for matching user input.</returns>
        IRecognize<T> Recognizer { get; }
    }

    /// <summary>
    /// The prompt that is returned by form prompter. 
    /// </summary>
    [Serializable]
    public sealed class FormPrompt : ICloneable
    {
        /// <summary>
        /// The text prompt that corresponds to Message.Text.
        /// </summary>
        public string Prompt { set; get; } = string.Empty;

        /// <summary>
        /// The buttons that will be mapped to Message.Attachments.
        /// </summary>
        public IList<FormButton> Buttons { set; get; } = new List<FormButton>();
        
        public override string ToString()
        {
            return $"{Prompt} {Language.BuildList(Buttons.Select(button => button.ToString()), Resources.DefaultChoiceSeparator, Resources.DefaultChoiceLastSeparator)}";
        }

        /// <summary>
        /// Deep clone the FormPrompt.
        /// </summary>
        /// <returns> A deep cloned instance of FormPrompt.</returns>
        public object Clone()
        {
            var newPrompt = new FormPrompt();
            newPrompt.Prompt = this.Prompt;
            newPrompt.Buttons = this.Buttons.Clone();
            return newPrompt;
        }
    }

    /// <summary>
    /// A Form button that will be mapped to Connector.Action.
    /// </summary>
    [Serializable]
    public sealed class FormButton : ICloneable
    {
        /// <summary>
        /// Picture which will appear on the button.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Message that will be sent to bot when this button is clicked.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Label of the button.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// URL which will be opened in the browser built-into Client application.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Clone the FormButton
        /// </summary>
        /// <returns> A new cloned instance of object.</returns>
        public object Clone()
        {
            return new FormButton
            {
                Image = this.Image,
                Message = this.Message,
                Title = this.Title,
                Url = this.Url
            };
        }

        /// <summary>
        /// ToString() override. 
        /// </summary>
        /// <returns> Title of the button.</returns>
        public override string ToString()
        {
            return Title; 
        }
    }

    public static partial class Extensions
    {
        internal static IList<FormButton> GenerateButtons<T>(this IEnumerable<T> options)
        {
            var buttons = new List<FormButton>();
            foreach (var option in options)
            {
                buttons.Add(new FormButton
                {
                    Title = option.ToString(),
                    Message = option.ToString()
                });
            }
            return buttons; 
        }

        internal static IList<Attachment> GenerateAttachments(this IList<FormButton> buttons, string text)
        {
            var actions = new List<CardAction>(); 
            foreach(var button in buttons)
            {
                CardAction action; 
                if (button.Url != null)
                {
                    action = new CardAction(ActionTypes.OpenUrl, button.Title, button.Image, button.Url);
                }
                else
                {
                    action = new CardAction(ActionTypes.ImBack, button.Title, button.Image, button.Message ?? button.Title);
                }

                actions.Add(action);
            }

            var attachments = new List<Attachment>();
            if (actions.Count > 0)
            {
                attachments.Add(new HeroCard(text: text, buttons: actions).ToAttachment());
            }
            return attachments;
        }

        internal static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> enumerable)
        {
            foreach (var cur in enumerable)
            {
                collection.Add(cur);
            }
        }

        internal static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
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
        /// <param name="fields">Fields name lookup.  (Defaults to forms.)</param>
        public Prompter(TemplateBaseAttribute annotation, IForm<T> form, IRecognize<T> recognizer, IFields<T> fields = null)
        {
            annotation.ApplyDefaults(form.Configuration.DefaultPrompt);
            _annotation = annotation;
            _form = form;
            _fields = fields ?? form.Fields;
            _recognizer = recognizer;
        }

        public TemplateBaseAttribute Annotation
        {
            get
            {
                return _annotation;
            }
        }

        public FormPrompt Prompt(T state, string pathName, params object[] args)
        {
            string currentChoice = null;
            string noValue = null;
            if (pathName != "")
            {
                var field = _fields.Field(pathName);
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
            IList<FormButton> buttons = new List<FormButton>(); 
            var response = ExpandTemplate(_annotation.Pattern(), currentChoice, noValue, state, pathName, args, ref buttons);
            return new FormPrompt {
                Prompt = (response == null ? "" : _spacesPunc.Replace(_spaces.Replace(Language.ANormalization(response), "$1 "), "$1")),
                Buttons = buttons
            };
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

        private string ExpandTemplate(string template, string currentChoice, string noValue, T state, string pathName, object[] args, ref IList<FormButton> buttons)
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
                    var name = expr.Substring(1);
                    if (name == "") name = pathName;
                    var pathField = _fields.Field(name);
                    substitute = Language.Normalize(pathField == null ? pathName : pathField.FieldDescription, _annotation.FieldCase);
                }
                else if (expr == "||")
                {
                    var builder = new StringBuilder();
                    var values = _recognizer.ValueDescriptions();
                    var useButtons = !field.AllowsMultiple && _annotation.ChoiceStyle == ChoiceStyleOptions.Auto;
                    if (values.Any() && _annotation.AllowDefault != BoolDefault.False && field.Optional)
                    {
                        values = values.Concat(new DescribeAttribute[] { new DescribeAttribute(Language.Normalize(noValue, _annotation.ChoiceCase)) });
                    }
                    string current = null;
                    if (_annotation.AllowDefault != BoolDefault.False)
                    {
                        if (!field.Optional)
                        {
                            if (!field.IsUnknown(state))
                            {
                                current = ExpandTemplate(currentChoice, null, noValue, state, pathName, args, ref buttons);
                            }
                        }
                        else
                        {
                            current = ExpandTemplate(currentChoice, null, noValue, state, pathName, args, ref buttons);
                        }
                    }
                    if (values.Any())
                    {
                        if (useButtons)
                        {
                            int i = 1;
                            foreach(var value in values)
                            {
                                var button = new FormButton() { Title = value.Description, Image = value.Image };
                                if (_annotation.AllowNumbers)
                                {
                                    button.Message = i.ToString();
                                }
                                buttons.Add(button);
                                ++i;
                            }
                        }
                        else
                        {
                            // Buttons do not support multiple selection so we fall back to text
                            if (((_annotation.ChoiceStyle == ChoiceStyleOptions.Auto || _annotation.ChoiceStyle == ChoiceStyleOptions.AutoText)
                                && values.Count() < 4)
                                || (_annotation.ChoiceStyle == ChoiceStyleOptions.Inline))
                            {
                                // Inline choices
                                if (_annotation.ChoiceParens == BoolDefault.True) builder.Append('(');
                                var choices = new List<string>();
                                var i = 1;
                                foreach (var value in values)
                                {
                                    choices.Add(string.Format(_annotation.ChoiceFormat, i, Language.Normalize(value.Description, _annotation.ChoiceCase)));
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
                                    builder.AppendFormat(_annotation.ChoiceFormat, i, Language.Normalize(value.Description, _annotation.ChoiceCase));
                                    ++i;
                                }
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
                    foreach (var entry in (from step in _fields where (!filled || !step.IsUnknown(state)) && step.Role == FieldRole.Value && step.Active(state) select step))
                    {
                        builder.Append("* ").AppendLine(format.Prompt(state, entry.Name).Prompt);
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
                        var eltDesc = _fields.Field(name);
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
                    var subValue = ExpandTemplate(expr.Substring(1), currentChoice, null, state, pathName, args, ref buttons);
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
                result = field.Prompt.Recognizer.ValueDescription(value).Description;
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
        private IFields<T> _fields;
        private TemplateBaseAttribute _annotation;
        private IRecognize<T> _recognizer;
    }
}
