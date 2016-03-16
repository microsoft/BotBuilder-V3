using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Form.Advanced
{

    public interface IPrompt<T>
    {
        PromptBase Annotation();
        string Prompt(T state, string path, params object[] args);
        IRecognizer Recognizer();
    }

    public class Prompter<T> : IPrompt<T>
        where T : class, new()
    {
        public Prompter(PromptBase annotation, IForm<T> form, IRecognizer recognizer)
        {
            annotation.ApplyDefaults(form.Configuration().DefaultPrompt);
            _annotation = annotation;
            _form = form;
            _fields = form.Fields();
            _recognizer = recognizer;
        }

        public virtual PromptBase Annotation()
        {
            return _annotation;
        }

        public virtual string Prompt(T state, string pathName, params object[] args)
        {
            bool expectsArgs;
            var templates = new Dictionary<TemplateUsage, IPrompt<T>>();
            if (pathName != "")
            {
                var field = _fields.Field(pathName);
                templates.Add(TemplateUsage.CurrentChoice,
                    new Prompter<T>(field.Template(TemplateUsage.CurrentChoice), _form, field.Prompt().Recognizer()));
                templates.Add(TemplateUsage.NoPreference,
                    new Prompter<T>(field.Template(TemplateUsage.NoPreference), _form, field.Prompt().Recognizer()));
            }
            var response = ExpandTemplate(_annotation.Template(), templates, state, pathName, false, args, out expectsArgs);
            return (response == null ? "" : _spaces.Replace(Language.ANormalization(expectsArgs ? string.Format(response, args) : response), "$1 "));
        }

        private string ExpandTemplate(string template, Dictionary<TemplateUsage, IPrompt<T>> templates, T state, string pathName, bool noUnspecified, object[] args, out bool expectsArgs)
        {
            expectsArgs = false;
            bool unspecified = false;
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
                    if (name == "")
                    {
                        // Use default pathname
                        name = pathName;
                    }
                    var pathField = _fields.Field(name);
                    substitute = Normalize(pathField == null ? pathName : pathField.Description(), PromptNormalization.Default);
                }
                else if (expr == "||")
                {
                    var builder = new StringBuilder();
                    var defaultValue = field.GetValue(state);
                    var values = _recognizer.ValueDescriptions();
                    var startNumber = 1;
                    if (_annotation.AllowDefault != BoolDefault.No)
                    {
                        if (!field.Optional())
                        {
                            if (!field.IsUnknown(state))
                            {
                                startNumber = 0;
                                values = new string[] { templates[TemplateUsage.CurrentChoice].Prompt(state, pathName) }.Concat(values);
                            }
                        }
                        else
                        {
                            startNumber = 0;
                            if (field.IsUnknown(state))
                            {
                                values = new string[] { templates[TemplateUsage.NoPreference].Prompt(state, pathName) }.Concat(values);
                            }
                            else
                            {
                                values = new string[] { templates[TemplateUsage.CurrentChoice].Prompt(state, pathName) }
                                    .Concat(values)
                                    .Concat(new string[] { templates[TemplateUsage.NoPreference].Prompt(state, pathName) });
                            }
                        }
                    }
                    if (values.Count() > 0)
                    {
                        if ((_annotation.Style == PromptStyle.Auto && values.Count() < 4)
                            || (_annotation.Style == PromptStyle.Inline))
                        {
                            // Inline choices
                            var i = startNumber;
                            bool first = true;
                            builder.Append('(');
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
                                builder.AppendFormat(_annotation.Format, i == 0 ? "c" : i.ToString(), value);
                                ++i;
                            }
                            builder.Append(')');
                        }
                        else
                        {
                            // Seperate line choices
                            var i = startNumber;
                            foreach (var value in values)
                            {
                                builder.Append("\n  ");
                                if (_annotation.AllowNumbers != BoolDefault.Yes)
                                {
                                    builder.Append("* ");
                                }
                                builder.AppendFormat(_annotation.Format, i == 0 ? "\n  c" : i.ToString(), value);
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
                    var prompt = new Prompt(_form.Configuration().StatusFormat);
                    if (match.Index > 0)
                    {
                        builder.Append("\n");
                    }
                    foreach (var entry in (from step in _fields where (!filled || !step.IsUnknown(state)) && step.Role() == FieldRole.Value && step.Active(state) select step))
                    {
                        builder.Append("* ").AppendLine(new Prompter<T>(prompt, _form, entry.Prompt().Recognizer()).Prompt(state, entry.Name()));
                    }
                    substitute = builder.ToString();
                }
                else if (expr.StartsWith("[") && expr.EndsWith("]"))
                {
                    // Generate a list from multiple fields
                    var paths = expr.Substring(1, expr.Length - 2).Split(' ');
                    var values = new List<Tuple<IField<T>, object>>();
                    foreach (var name in paths)
                    {
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
                        var elements = (from elt in values select Normalize(ValueDescription(elt.Item1, elt.Item2), PromptNormalization.Default)).ToArray();
                        substitute = Language.BuildList(elements, _annotation.Separator, _annotation.LastSeparator);
                    }
                }
                else if (expr.StartsWith("?"))
                {
                    // Conditional template
                    bool subExpects;
                    var subValue = ExpandTemplate(expr.Substring(1), templates, state, pathName, true, args, out subExpects);
                    if (subValue == null)
                    {
                        substitute = "";
                    }
                    else
                    {
                        expectsArgs = expectsArgs || subExpects;
                        substitute = subValue;
                    }
                }
                else if (int.TryParse(expr, out numeric))
                {
                    // Pass through numeric format strings
                    expectsArgs = true;
                    if (numeric < args.Length)
                    {
                        substitute = "{" + expr + "}";
                    }
                    else
                    {
                        unspecified = true;
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
                        if (noUnspecified)
                        {
                            unspecified = true;
                            break;
                        }
                        substitute = _form.Configuration().Unspecified;
                    }
                    else
                    {
                        var value = pathDesc.GetValue(state);
                        if (value.GetType() != typeof(string) && value.GetType().IsIEnumerable())
                        {
                            var values = (value as System.Collections.IEnumerable);
                            substitute = Language.BuildList(from elt in values.Cast<object>() select Normalize(ValueDescription(pathDesc, elt), PromptNormalization.Default),
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
            return (unspecified ? null : response.Append(template.Substring(last, template.Length - last)).ToString());
        }

        private string ValueDescription(IField<T> field, object value)
        {
            return field.Prompt().Recognizer().ValueDescription(value);
        }

        public virtual IRecognizer Recognizer()
        {
            return _recognizer;
        }

        private string Normalize(string value, PromptNormalization auto)
        {
            switch (_annotation.Case)
            {
                case PromptNormalization.Auto:
                    switch (auto)
                    {
                        case PromptNormalization.Lower: value = value.ToLower(); break;
                        case PromptNormalization.Upper: value = value.ToUpper(); break;
                    }
                    break;
                case PromptNormalization.Lower: value = value.ToLower(); break;
                case PromptNormalization.Upper: value = value.ToUpper(); break;
                case PromptNormalization.Default: break;
            }
            return value;
        }

        private static Regex _args = new Regex(@"{((?>[^{}]+|{(?<number>)|}(?<-number>))*(?(number)(?!)))}", RegexOptions.Compiled);
        private static Regex _spaces = new Regex(@"(\S)( {2,})", RegexOptions.Compiled);
        private IForm<T> _form;
        private IFields<T> _fields;
        private PromptBase _annotation;
        private IRecognizer _recognizer;
    }
}
