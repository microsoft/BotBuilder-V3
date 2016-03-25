using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Bot.Builder.Form.Advanced;

namespace Microsoft.Bot.Builder.Form
{
    public sealed class FormBuilder<T> : IFormBuilder<T>
         where T : class, new()
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

        public static IFormBuilder<T> Start(bool ignoreAnnotations = false)
        {
            return new FormBuilder<T>(ignoreAnnotations);
        }

        IForm<T> IFormBuilder<T>.Build()
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

        FormConfiguration IFormBuilder<T>.Configuration { get { return _form._configuration; } }

        IFormBuilder<T> IFormBuilder<T>.Message(string message, ConditionalDelegate<T> condition)
        {
            _form._steps.Add(new MessageStep<T>(new Prompt(message), condition, _form));
            return this;
        }

        IFormBuilder<T> IFormBuilder<T>.Message(Prompt prompt, ConditionalDelegate<T> condition)
        {
            _form._steps.Add(new MessageStep<T>(prompt, condition, _form));
            return this;
        }

        IFormBuilder<T> IFormBuilder<T>.Field(string name, ConditionalDelegate<T> condition, ValidateDelegate<T> validate)
        {
            var field = (condition == null ? new FieldReflector<T>(name, _form) : new Conditional<T>(name, _form, condition));
            if (validate != null)
            {
                field.Validate(validate);
            }
            return AddField(field);
        }

        IFormBuilder<T> IFormBuilder<T>.Field(string name, string prompt, ConditionalDelegate<T> condition, ValidateDelegate<T> validate)
        {
            var field = (condition == null ? new FieldReflector<T>(name, _form) : new Conditional<T>(name, _form, condition));
            if (validate != null)
            {
                field.Validate(validate);
            }
            field.Prompt(new Prompt(prompt));
            return AddField(field);
        }

        IFormBuilder<T> IFormBuilder<T>.Field(string name, Prompt prompt, ConditionalDelegate<T> condition, ValidateDelegate<T> validate)
        {
            var field = (condition == null ? new FieldReflector<T>(name, _form) : new Conditional<T>(name, _form, condition));
            if (validate != null)
            {
                field.Validate(validate);
            }
            field.Prompt(prompt);
            return AddField(field);
        }

        IFormBuilder<T> IFormBuilder<T>.Field(IField<T> field)
        {
            return AddField(field);
        }

        IFormBuilder<T> IFormBuilder<T>.AddRemainingFields(IEnumerable<string> exclude)
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

        IFormBuilder<T> IFormBuilder<T>.Confirm(string prompt, ConditionalDelegate<T> condition, IEnumerable<string> dependencies)
        {
            IFormBuilder<T> builder = this;
            return builder.Confirm(new Prompt(prompt) { ChoiceFormat = "{1}", AllowDefault = BoolDefault.False }, condition, dependencies);
        }

        IFormBuilder<T> IFormBuilder<T>.Confirm(Prompt prompt, ConditionalDelegate<T> condition, IEnumerable<string> dependencies)
        {
            if (condition == null) condition = (state) => true;
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

        IFormBuilder<T> IFormBuilder<T>.Confirm(IFieldPrompt<T> prompt)
        {
            // TODO: Need to fill this in
            return this;
        }

        IFormBuilder<T> IFormBuilder<T>.OnCompletion(CompletionDelegate<T> callback)
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
