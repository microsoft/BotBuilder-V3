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
using System.Reflection;

using Microsoft.Bot.Builder.FormFlow.Advanced;

namespace Microsoft.Bot.Builder.FormFlow
{
    #region Documentation
    /// <summary>   Build a form by specifying messages, fields and confirmations.</summary>
    /// <typeparam name="T">    Form state class. </typeparam>
    #endregion
    public sealed class FormBuilder<T> : IFormBuilder<T>
        where T : class
    {
        private readonly Form<T> _form;

        /// <summary>
        /// Construct the form builder.
        /// </summary>
        /// <param name="ignoreAnnotations">True if you want to ignore any annotations on classes when doing reflection.</param>
        public FormBuilder(bool ignoreAnnotations = false)
        {
            _form = new Form<T>(ignoreAnnotations);
        }

        public IForm<T> Build()
        {
            if (!_form._steps.Any((step) => step.Type == StepType.Field))
            {
                var paths = new List<string>();
                FormBuilder<T>.FieldPaths(typeof(T), "", paths);
                IFormBuilder<T> builder = this;
                foreach (var path in paths)
                {
                    builder.Field(new FieldReflector<T>(path));
                }
                builder.Confirm("Is this your selection?\n{*}");
            }
            Validate();
            return this._form;
        }

        public FormConfiguration Configuration { get { return _form._configuration; } }

        public IFormBuilder<T> Message(string message, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            _form._steps.Add(new MessageStep<T>(new PromptAttribute(message), condition, dependencies, _form));
            return this;
        }

        public IFormBuilder<T> Message(PromptAttribute prompt, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            _form._steps.Add(new MessageStep<T>(prompt, condition, dependencies, _form));
            return this;
        }

        public IFormBuilder<T> Message(MessageDelegate<T> generateMessage, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            _form._steps.Add(new MessageStep<T>(generateMessage, condition, dependencies, _form));
            return this;
        }

        public IFormBuilder<T> Field(string name, ActiveDelegate<T> condition = null, ValidateAsyncDelegate<T> validate = null)
        {
            var field = (condition == null ? new FieldReflector<T>(name) : new Conditional<T>(name, condition));
            if (validate != null)
            {
                field.SetValidate(validate);
            }
            return AddField(field);
        }

        public IFormBuilder<T> Field(string name, string prompt, ActiveDelegate<T> condition = null, ValidateAsyncDelegate<T> validate = null)
        {
            var field = (condition == null ? new FieldReflector<T>(name) : new Conditional<T>(name, condition));
            if (validate != null)
            {
                field.SetValidate(validate);
            }
            field.SetPrompt(new PromptAttribute(prompt));
            return AddField(field);
        }

        public IFormBuilder<T> Field(string name, PromptAttribute prompt, ActiveDelegate<T> condition = null, ValidateAsyncDelegate<T> validate = null)
        {
            var field = (condition == null ? new FieldReflector<T>(name) : new Conditional<T>(name, condition));
            if (validate != null)
            {
                field.SetValidate(validate);
            }
            field.SetPrompt(prompt);
            return AddField(field);
        }

        public IFormBuilder<T> Field(IField<T> field)
        {
            return AddField(field);
        }

        public IFormBuilder<T> AddRemainingFields(IEnumerable<string> exclude = null)
        {
            var exclusions = (exclude == null ? new string[0] : exclude.ToArray());
            var paths = new List<string>();
            FieldPaths(typeof(T), "", paths);
            foreach (var path in paths)
            {
                if (!exclusions.Contains(path))
                {
                    IField<T> field = _form._fields.Field(path);
                    if (field == null)
                    {
                        AddField(new FieldReflector<T>(path));
                    }
                }
            }
            return this;
        }

        public IFormBuilder<T> Confirm(string prompt, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            IFormBuilder<T> builder = this;
            return builder.Confirm(new PromptAttribute(prompt) { ChoiceFormat = Resources.ConfirmChoiceFormat, AllowDefault = BoolDefault.False }, condition, dependencies);
        }

        public IFormBuilder<T> Confirm(PromptAttribute prompt, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            if (condition == null) condition = state => true;
            dependencies = dependencies ?? _form.Dependencies(_form.Steps.Count());
            var confirmation = new Confirmation<T>(prompt, condition, dependencies, _form);
            confirmation.Form = _form;
            _form._fields.Add(confirmation);
            _form._steps.Add(new ConfirmStep<T>(confirmation));
            return this;
        }

        public IFormBuilder<T> Confirm(MessageDelegate<T> generateMessage, ActiveDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            if (condition == null) condition = state => true;
            dependencies = dependencies ?? _form.Dependencies(_form.Steps.Count());
            var confirmation = new Confirmation<T>(generateMessage, condition, dependencies, _form);
            confirmation.Form = _form;
            _form._fields.Add(confirmation);
            _form._steps.Add(new ConfirmStep<T>(confirmation));
            return this;
        }

        public IFormBuilder<T> OnCompletionAsync(OnCompletionAsyncDelegate<T> callback)
        {
            _form._completion = callback;
            return this;
        }

        private IFormBuilder<T> AddField(IField<T> field)
        {
            field.Form = _form;
            _form._fields.Add(field);
            var step = new FieldStep<T>(field.Name, _form);
            var stepIndex = this._form._steps.FindIndex(s => s.Name == field.Name);
            if (stepIndex >= 0)
            {
                _form._steps[stepIndex] = step;
            }
            else
            {
                _form._steps.Add(step);
            }
            return this;
        }

        private Dictionary<TemplateUsage, int> _templateArgs = new Dictionary<TemplateUsage, int>
        {
            {TemplateUsage.Bool, 0 },
            { TemplateUsage.BoolHelp, 1},
            { TemplateUsage.Clarify, 1},
            { TemplateUsage.CurrentChoice, 0},
            { TemplateUsage.DateTime, 0},
            { TemplateUsage.DateTimeHelp, 2},
            { TemplateUsage.Double, 2},
            { TemplateUsage.DoubleHelp, 4},
            { TemplateUsage.EnumManyNumberHelp, 3},
            { TemplateUsage.EnumOneNumberHelp, 3},
            { TemplateUsage.EnumManyWordHelp, 3},
            { TemplateUsage.EnumOneWordHelp, 3},
            { TemplateUsage.EnumSelectOne, 0},
            { TemplateUsage.EnumSelectMany, 0},
            { TemplateUsage.Feedback, 1},
            { TemplateUsage.Help, 2},
            { TemplateUsage.HelpClarify, 2},
            { TemplateUsage.HelpConfirm, 2},
            { TemplateUsage.HelpNavigation, 2},
            { TemplateUsage.Integer, 2},
            { TemplateUsage.IntegerHelp, 4},
            { TemplateUsage.Navigation, 0},
            { TemplateUsage.NavigationCommandHelp, 1},
            { TemplateUsage.NavigationFormat, 0},
            { TemplateUsage.NavigationHelp, 2},
            { TemplateUsage.NoPreference, 0},
            { TemplateUsage.NotUnderstood, 1},
            { TemplateUsage.StatusFormat, 0},
            { TemplateUsage.String, 0},
            { TemplateUsage.StringHelp, 2},
            { TemplateUsage.Unspecified, 0},
        };

        private int TemplateArgs(TemplateUsage usage)
        {
            int args;
            if (!_templateArgs.TryGetValue(usage, out args))
            {
                throw new ArgumentException("Missing template usage for validation");
            }
            return args;
        }

        private void Validate()
        {
            foreach (var step in _form._steps)
            {
                // Validate prompt
                var annotation = step.Annotation;
                if (annotation != null)
                {
                    var name = step.Type == StepType.Field ? step.Name : "";
                    foreach (var pattern in annotation.Patterns)
                    {
                        ValidatePattern(pattern, name, 5);
                    }
                    if (step.Type != StepType.Message)
                    {
                        foreach (TemplateUsage usage in Enum.GetValues(typeof(TemplateUsage)))
                        {
                            if (usage != TemplateUsage.None)
                            {
                                foreach (var pattern in step.Field.Template(usage).Patterns)
                                {
                                    ValidatePattern(pattern, name, TemplateArgs(usage));
                                }
                            }
                        }
                    }
                }
            }
            ValidatePattern(_form.Configuration.DefaultPrompt.ChoiceFormat, "", 2);
        }

        private void ValidatePattern(string pattern, string pathName, int maxArgs)
        {
            if (!Prompter<T>.ValidatePattern(_form, pattern, pathName, maxArgs))
            {
                throw new ArgumentException(string.Format("Illegal pattern: \"{0}\"", pattern));
            }
        }

        internal static void FieldPaths(Type type, string path, List<string> paths)
        {
            var newPath = (path == "" ? path : path + ".");
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                TypePaths(field.FieldType, newPath + field.Name, paths);
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.CanRead && property.CanWrite)
                {
                    TypePaths(property.PropertyType, newPath + property.Name, paths);
                }
            }
        }

        internal static void TypePaths(Type type, string path, List<string> paths)
        {
            if (type.IsClass)
            {
                if (type == typeof(string))
                {
                    paths.Add(path);
                }
                else if (type.IsIEnumerable())
                {
                    var elt = type.GetGenericElementType();
                    if (elt.IsEnum)
                    {
                        paths.Add(path);
                    }
                    else
                    {
                        // TODO: What to do about enumerations of things other than enums?
                    }
                }
                else
                {
                    FieldPaths(type, path, paths);
                }
            }
            else if (type.IsEnum)
            {
                paths.Add(path);
            }
            else if (type == typeof(bool))
            {
                paths.Add(path);
            }
            else if (type.IsIntegral())
            {
                paths.Add(path);
            }
            else if (type.IsDouble())
            {
                paths.Add(path);
            }
            else if (type.IsNullable() && type.IsValueType)
            {
                paths.Add(path);
            }
            else if (type == typeof(DateTime))
            {
                paths.Add(path);
            }
        }
    }
}
