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

using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Builder.Resource;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Bot.Builder.FormFlow
{
    #region Documentation
    /// <summary>   Build a form by specifying messages, fields and confirmations.</summary>
    #endregion
    public sealed class FormJsonBuilder: FormBuilderBase<JObject>
    {
        private readonly bool _ignoreAnnotations;
        private readonly JObject _schema;

        public FormJsonBuilder(JObject schema, bool ignoreAnnotations = false)
            : base()
        {
            _ignoreAnnotations = ignoreAnnotations;
            _schema = schema;
        }

        public override IForm<JObject> Build(Assembly resourceAssembly = null, string resourceName = null)
        {
            /*
            if (!_form._steps.Any((step) => step.Type == StepType.Field))
            {
                var paths = new List<string>();
                FieldPaths(typeof(T), "", paths);
                foreach (var path in paths)
                {
                    Field(new FieldReflector<JObject>(path, _ignoreAnnotations));
                }
                Confirm(new PromptAttribute(_form.Configuration.Template(TemplateUsage.Confirmation)));
            }
            */
            return base.Build(resourceAssembly, resourceName);
        }

        internal static void Fields(JObject schema, string prefix, IList<string> fields)
        {
            if (schema["properties"] != null)
            {
                foreach (JProperty property in schema["properties"])
                {
                    var path = (prefix == null ? property.Name : $"{prefix}.{property.Name}");
                    var childSchema = (JObject)property.Value;
                    var eltSchema = FieldJson.ElementSchema(childSchema);
                    if (FieldJson.IsPrimitiveType(eltSchema))
                    {
                        fields.Add(path);
                    }
                    else
                    {
                        Fields(childSchema, path, fields);
                    }
                }
            }
        }

        /// <summary>
        /// Add any remaining fields.
        /// </summary>
        /// <param name="exclude">Fields not to include.</param>
        /// <returns>Modified <see cref="IFormBuilder{T}"/>.</returns>
        /// <remarks>
        /// See <see cref="FieldJson"/> for a description of JSON Schema extensions 
        /// for defining your fields and OnCompletion handler.
        /// </remarks>
        public override IFormBuilder<JObject> AddRemainingFields(IEnumerable<string> exclude = null)
        {
            var exclusions = (exclude == null ? Array.Empty<string>() : exclude.ToArray());
            var fields = new List<string>();
            Fields(_schema, null, fields);
            foreach (var field in fields)
            {
                if (!exclusions.Contains(field) && !HasField(field))
                {
                    Field(field);
                }
            }
            return this;
        }

        /// <summary>
        /// Define a step for filling in a particular value in a JObject as defined by a JSON Schema.
        /// </summary>
        /// <param name="name">Path in the form state to the value being filled in.</param>
        /// <param name="active">Delegate to test form state to see if step is active.</param>
        /// <param name="validate">Delegate to validate the field value.</param>
        /// <returns>This form.</returns>
        /// <remarks>
        /// See <see cref="FieldJson"/> for a description of JSON Schema extensions 
        /// for defining your fields.
        /// </remarks>
        public override IFormBuilder<JObject> Field(string name, ActiveDelegate<JObject> active = null, ValidateAsyncDelegate<JObject> validate = null)
        {
            var field = new FieldJson(_schema, name);
            if (active != null)
            {
                field.SetActive(active);
            }
            if (validate != null)
            {
                field.SetValidate(validate);
            }
            AddSteps(field.Before);
            Field(field);
            AddSteps(field.After);
            return this;
        }

        private void AddSteps(IEnumerable<FieldJson.MessageOrConfirmation> steps)
        {
            foreach (var step in steps)
            {
                if (step.IsMessage)
                {
                    if (step.MessageGenerator != null)
                    {
                        Message(step.MessageGenerator, step.Condition, step.Dependencies);
                    }
                    else
                    {
                        Message(step.Prompt, step.Condition, step.Dependencies);
                    }
                }
                else
                {
                    if (step.MessageGenerator != null)
                    {
                        Confirm(step.MessageGenerator, step.Condition, step.Dependencies);
                    }
                    else
                    {
                        Confirm(step.Prompt, step.Condition, step.Dependencies);
                    }
                }
            }
        }

        /// <summary>
        /// Define a step for filling in a particular value in a JObject as defined by a JSON Schema.
        /// </summary>
        /// <param name="name">Path in the form state to the value being filled in.</param>
        /// <param name="prompt">Simple \ref patterns to describe prompt for field.</param>
        /// <param name="active">Delegate to test form state to see if step is active.n</param>
        /// <param name="validate">Delegate to validate the field value.</param>
        /// <returns>This form.</returns>
        /// <remarks>
        /// See <see cref="FieldJson"/> for a description of JSON Schema extensions 
        /// for defining your fields.
        /// </remarks>
        public override IFormBuilder<JObject> Field(string name, string prompt, ActiveDelegate<JObject> active = null, ValidateAsyncDelegate<JObject> validate = null)
        {
            return Field(name, new PromptAttribute(prompt), active, validate);
        }

        /// <summary>
        /// Define a step for filling in a particular value in a JObject as defined by a JSON Schema.
        /// </summary>
        /// <param name="name">Path in the form state to the value being filled in.</param>
        /// <param name="prompt">Prompt pattern with more formatting control to describe prompt for field.</param>
        /// <param name="active">Delegate to test form state to see if step is active.n</param>
        /// <param name="validate">Delegate to validate the field value.</param>
        /// <returns>This form.</returns>
        /// <remarks>
        /// See <see cref="FieldJson"/> for a description of JSON Schema extensions 
        /// for defining your fields.
        /// </remarks>
        public override IFormBuilder<JObject> Field(string name, PromptAttribute prompt, ActiveDelegate<JObject> active = null, ValidateAsyncDelegate<JObject> validate = null)
        {
            var field = new FieldJson(_schema, name);
            field.SetPrompt(prompt);
            if (active != null)
            {
                field.SetActive(active);
            }
            if (validate != null)
            {
                field.SetValidate(validate);
            }
            return Field(field);
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