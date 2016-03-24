using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Bot.Builder.Form.Advanced;

namespace Microsoft.Bot.Builder.Form
{
    public sealed class FormModelBuilder<T> : IFormModelBuilder<T>
         where T : class, new()
    {
        private readonly FormModel<T> _model;

        /// <summary>
        /// Construct the form model builder.
        /// </summary>
        /// <param name="ignoreAnnotations">True if you want to ignore any annotations on classes when doing reflection.</param>
        public FormModelBuilder(bool ignoreAnnotations = false)
        {
            _model = new FormModel<T>(ignoreAnnotations);
        }

        public static IFormModelBuilder<T> Start(bool ignoreAnnotations = false)
        {
            return new FormModelBuilder<T>(ignoreAnnotations);
        }

        IFormModel<T> IFormModelBuilder<T>.Build()
        {
            if (!_model._steps.Any((step) => step.Type == StepType.Field))
            {
                var paths = new List<string>(); 
                FormModelBuilder<T>.FieldPaths(typeof(T), "", paths);
                IFormModelBuilder<T> builder = this;
                foreach (var path in paths)
                {
                    builder.Field(new FieldReflector<T>(path, _model));
                }
                builder.Confirm("Is ths your selection?\n{*}");
            }

            return this._model;
        }

        FormConfiguration IFormModelBuilder<T>.Configuration { get { return _model._configuration; } }

        IFormModelBuilder<T> IFormModelBuilder<T>.Message(string message, ConditionalDelegate<T> condition)
        {
            _model._steps.Add(new MessageStep<T>(new Prompt(message), condition, _model));
            return this;
        }

        IFormModelBuilder<T> IFormModelBuilder<T>.Message(Prompt prompt, ConditionalDelegate<T> condition)
        {
            _model._steps.Add(new MessageStep<T>(prompt, condition, _model));
            return this;
        }

        IFormModelBuilder<T> IFormModelBuilder<T>.Field(string name, ConditionalDelegate<T> condition, ValidateDelegate<T> validate)
        {
            var field = (condition == null ? new FieldReflector<T>(name, _model) : new Conditional<T>(name, _model, condition));
            if (validate != null)
            {
                field.Validate(validate);
            }
            return AddField(field);
        }

        IFormModelBuilder<T> IFormModelBuilder<T>.Field(string name, string prompt, ConditionalDelegate<T> condition, ValidateDelegate<T> validate)
        {
            var field = (condition == null ? new FieldReflector<T>(name, _model) : new Conditional<T>(name, _model, condition));
            if (validate != null)
            {
                field.Validate(validate);
            }
            field.Prompt(new Prompt(prompt));
            return AddField(field);
        }

        IFormModelBuilder<T> IFormModelBuilder<T>.Field(string name, Prompt prompt, ConditionalDelegate<T> condition, ValidateDelegate<T> validate)
        {
            var field = (condition == null ? new FieldReflector<T>(name, _model) : new Conditional<T>(name, _model, condition));
            if (validate != null)
            {
                field.Validate(validate);
            }
            field.Prompt(prompt);
            return AddField(field);
        }

        IFormModelBuilder<T> IFormModelBuilder<T>.Field(IField<T> field)
        {
            return AddField(field);
        }

        IFormModelBuilder<T> IFormModelBuilder<T>.AddRemainingFields(IEnumerable<string> exclude)
        {
            var exclusions = (exclude == null ? new string[0] : exclude.ToArray());
            var paths = new List<string>();
            FieldPaths(typeof(T), "", paths);
            foreach (var path in paths)
            {
                if (!exclusions.Contains(path))
                {
                    IField<T> field = _model._fields.Field(path);
                    if (field == null)
                    {
                        AddField(new FieldReflector<T>(path, _model));
                    }
                }
            }
            return this;
        }

        IFormModelBuilder<T> IFormModelBuilder<T>.Confirm(string prompt, ConditionalDelegate<T> condition, IEnumerable<string> dependencies)
        {
            IFormModelBuilder<T> builder = this;
            return builder.Confirm(new Prompt(prompt) { AllowNumbers = BoolDefault.False, AllowDefault = BoolDefault.False }, condition, dependencies);
        }

        IFormModelBuilder<T> IFormModelBuilder<T>.Confirm(Prompt prompt, ConditionalDelegate<T> condition, IEnumerable<string> dependencies)
        {
            if (condition == null) condition = (state) => true;
            if (dependencies == null)
            {
                // Default next steps go from previous field ignoring confirmations back to next confirmation
                // Last field before confirmation
                var end = _model._steps.Count();
                while (end > 0)
                {
                    if (_model._steps[end - 1].Type == StepType.Field)
                    {
                        break;
                    }
                    --end;
                }

                var start = end;
                while (start > 0)
                {
                    if (_model._steps[start - 1].Type == StepType.Confirm)
                    {
                        break;
                    }
                    --start;
                }
                var fields = new List<string>();
                for (var i = start; i < end; ++i)
                {
                    if (_model._steps[i].Type == StepType.Field)
                    {
                        fields.Add(_model._steps[i].Name);
                    }
                }
                dependencies = fields;
            }
            var confirmation = new Confirmation<T>(prompt, condition, dependencies, _model);
            _model._fields.Add(confirmation);
            _model._steps.Add(new ConfirmStep<T>(confirmation));
            return this;
        }

        IFormModelBuilder<T> IFormModelBuilder<T>.Confirm(IFieldPrompt<T> prompt)
        {
            // TODO: Need to fill this in
            return this;
        }

        IFormModelBuilder<T> IFormModelBuilder<T>.OnCompletion(CompletionDelegate<T> callback)
        {
            _model._completion = callback;
            return this;
        }

        private IFormModelBuilder<T> AddField(IField<T> field)
        {
            _model._fields.Add(field);
            var step = new FieldStep<T>(field.Name, _model);
            var stepIndex = this._model._steps.FindIndex(s => s.Name == field.Name);
            if (stepIndex >= 0)
            {
                _model._steps[stepIndex] = step;
            }
            else
            {
                _model._steps.Add(step);
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
