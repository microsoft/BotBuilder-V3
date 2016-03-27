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

using Microsoft.Bot.Builder.Form.Advanced;

namespace Microsoft.Bot.Builder.Form
{
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
                    builder.Field(new FieldReflector<T>(path, _form));
                }
                builder.Confirm("Is this your selection?\n{*}");
            }

            return this._form;
        }

        public FormConfiguration Configuration { get { return _form._configuration; } }

        public IFormBuilder<T> Message(string message, ConditionalDelegate<T> condition = null)
        {
            _form._steps.Add(new MessageStep<T>(new Prompt(message), condition, _form));
            return this;
        }

        public IFormBuilder<T> Message(Prompt prompt, ConditionalDelegate<T> condition = null)
        {
            _form._steps.Add(new MessageStep<T>(prompt, condition, _form));
            return this;
        }

        public IFormBuilder<T> Field(string name, ConditionalDelegate<T> condition = null, ValidateDelegate<T> validate = null)
        {
            var field = (condition == null ? new FieldReflector<T>(name, _form) : new Conditional<T>(name, _form, condition));
            if (validate != null)
            {
                field.Validate(validate);
            }
            return AddField(field);
        }

        public IFormBuilder<T> Field(string name, string prompt, ConditionalDelegate<T> condition = null, ValidateDelegate<T> validate = null)
        {
            var field = (condition == null ? new FieldReflector<T>(name, _form) : new Conditional<T>(name, _form, condition));
            if (validate != null)
            {
                field.Validate(validate);
            }
            field.Prompt(new Prompt(prompt));
            return AddField(field);
        }

        public IFormBuilder<T> Field(string name, Prompt prompt, ConditionalDelegate<T> condition = null, ValidateDelegate<T> validate = null)
        {
            var field = (condition == null ? new FieldReflector<T>(name, _form) : new Conditional<T>(name, _form, condition));
            if (validate != null)
            {
                field.Validate(validate);
            }
            field.Prompt(prompt);
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
                        AddField(new FieldReflector<T>(path, _form));
                    }
                }
            }
            return this;
        }

        public IFormBuilder<T> Confirm(string prompt, ConditionalDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            IFormBuilder<T> builder = this;
            return builder.Confirm(new Prompt(prompt) { ChoiceFormat = "{1}", AllowDefault = BoolDefault.False }, condition, dependencies);
        }

        public IFormBuilder<T> Confirm(Prompt prompt, ConditionalDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            if (condition == null) condition = state => true;
            if (dependencies == null)
            {
                // Default next steps go from previous field ignoring confirmations back to next confirmation
                // Last field before confirmation
                var end = _form._steps.Count();
                while (end > 0)
                {
                    if (_form._steps[end - 1].Type == StepType.Field)
                    {
                        break;
                    }
                    --end;
                }

                var start = end;
                while (start > 0)
                {
                    if (_form._steps[start - 1].Type == StepType.Confirm)
                    {
                        break;
                    }
                    --start;
                }
                var fields = new List<string>();
                for (var i = start; i < end; ++i)
                {
                    if (_form._steps[i].Type == StepType.Field)
                    {
                        fields.Add(_form._steps[i].Name);
                    }
                }
                dependencies = fields;
            }
            var confirmation = new Confirmation<T>(prompt, condition, dependencies, _form);
            _form._fields.Add(confirmation);
            _form._steps.Add(new ConfirmStep<T>(confirmation));
            return this;
        }

        public IFormBuilder<T> Confirm(IFieldPrompt<T> prompt)
        {
            // TODO: Need to fill this in
            return this;
        }

        public IFormBuilder<T> OnCompletionAsync(CompletionDelegate<T> callback)
        {
            _form._completion = callback;
            return this;
        }

        private IFormBuilder<T> AddField(IField<T> field)
        {
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
