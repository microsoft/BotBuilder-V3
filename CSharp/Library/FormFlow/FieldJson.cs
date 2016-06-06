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

using Microsoft.Bot.Builder.FormFlow.Advanced;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.FormFlow.Advanced
{
    /// <summary>
    /// %Field defined through JSON Schema.
    /// </summary>
    /// <remarks>
    /// This defines a field where the definition is driven through [JSON Schema](http://json-schema.org/latest/json-schema-validation.html)
    /// with optional additional annotations that correspond to the attributes provided for C#.  The standard properties that are used
    /// from JSON Schema include:
    /// * `type` -- Defines the fields type.
    /// * `enum` -- Defines the possible field values.
    /// * `minimum` -- Defines the minimum allowed value as described in <see cref="NumericAttribute"/>.
    /// * `maximum` -- Defines the maximum allowed value as described in <see cref="NumericAttribute"/>.
    /// * `required` -- Defines what fields are required.
    /// 
    /// Templates and prompts use the same vocabulary as <see cref="TemplateAttribute"/> and <see cref="PromptAttribute"/>.  
    /// The property names are the same and the values are the same as those in the underlying C# enumeration.  
    /// For example to define a template to override the <see cref="TemplateUsage.NotUnderstood"/> template
    /// and specify a <see cref="TemplateBaseAttribute.ChoiceStyle"/> you would put this in your schema: 
    /// `"Templates":{ "NotUnderstood": { Patterns: ["I don't get it"], "ChoiceStyle":"Auto"}}`.
    /// 
    /// %Extensions defined at the root of a schema or as a peer of the "type" property.  
    /// * `Templates:{TemplateUsage: { Patterns:[string, ...], &lt;args&gt; }, ...}` -- Define templates.
    /// * `Prompt: { Patterns:[string, ...] &lt;args&gt;}` -- Define a prompt.
    /// 
    /// %Extensions that are found in a property description as peers to the "type" property of a JSON Schema.
    /// * `DateTime:bool` -- Marks a field as being a DateTime field.
    /// * `Describe:string` -- Description of a field.
    /// * `Terms:[string ,...]` -- Regular expressions for matching a field.
    /// * `MaxPhrase:int` -- This will run your terms through <see cref="Language.GenerateTerms(string, int)"/> to expand them.
    /// * `Values:{ string: {Describe:string, Terms:[string, ...], MaxPhrase}, ...}` -- The string must be found in the types "enum" and this allows you to override the automatically generated descriptions and terms.  If MaxPhrase is specified the terms are passed through <see cref="Language.GenerateTerms(string, int)"/>.
    /// 
    /// %Fields defined through this class have the same ability to extend or override the definitions
    /// programatically as any other field.  They can also be localized in the same way.
    /// </remarks>
    public class FieldJson : Field<JObject>
    {
        /// <summary>
        /// Construct a field from a JSON schema.
        /// </summary>
        /// <param name="schema">Schema for object.</param>
        /// <param name="name">Name of field within schema.</param>
        public FieldJson(JObject schema, string name)
            : base(name, FieldRole.Value)
        {
            _schema = schema;
            bool optional;
            var fieldSchema = FieldSchema(name, out optional);
            var eltSchema = ElementSchema(fieldSchema);
            ProcessAnnotations(schema, fieldSchema, eltSchema);
            var fieldName = name.Split('.').Last();
            JToken date;
            if (eltSchema.TryGetValue("DateTime", out date) && date.Value<bool>())
            {
                SetType(typeof(DateTime));
            }
            else
            {
                SetType(eltSchema["enum"] != null && eltSchema["enum"].Any() ? null : ToType(eltSchema));
            }
            SetAllowsMultiple(IsType(fieldSchema, "array"));
            SetFieldDescription(AString(fieldSchema, "Describe") ?? Language.CamelCase(fieldName));
            var terms = Strings(fieldSchema, "Terms");
            JToken maxPhrase;
            if (terms != null && fieldSchema.TryGetValue("MaxPhrase", out maxPhrase))
            {
                terms = (from seed in terms
                         from gen in Language.GenerateTerms(seed, (int)maxPhrase)
                         select gen).ToArray<string>();
            }
            SetFieldTerms(terms ?? Language.GenerateTerms(Language.CamelCase(fieldName), 3));
            ProcessEnum(eltSchema);
            SetOptional(optional);
            SetIsNullable(IsType(fieldSchema, "null"));
        }

        #region IFieldState
        public override object GetValue(JObject state)
        {
            object result = null;
            var val = state.SelectToken(_name);
            if (val != null)
            {
                if (_type == null)
                {
                    if (_allowsMultiple)
                    {
                        result = val.ToObject<string[]>();
                    }
                    else
                    {
                        result = (string)val;
                    }
                }
                else
                {
                    result = val.ToObject(_type);
                }
            }
            return result;
        }

        public override void SetValue(JObject state, object value)
        {
            var jvalue = JToken.FromObject(value);
            var current = state.SelectToken(_name);
            if (current == null)
            {
                var step = state;
                var steps = _name.Split('.');
                foreach (var part in steps.Take(steps.Count() - 1))
                {
                    var next = step.GetValue(part);
                    if (next == null)
                    {
                        var nextStep = new JObject();
                        step.Add(part, nextStep);
                        step = nextStep;
                    }
                    else
                    {
                        step = (JObject)next;
                    }
                }
                step.Add(steps.Last(), jvalue);
            }
            else
            {
                current.Replace(jvalue);
            }
        }

        public override bool IsUnknown(JObject state)
        {
            return state.SelectToken(_name) == null;
        }

        public override void SetUnknown(JObject state)
        {
            var token = state.SelectToken(_name);
            if (token != null)
            {
                token.Parent.Remove();
            }
        }

        #endregion

        protected JObject FieldSchema(string path, out bool optional)
        {
            var schema = _schema;
            var parts = path.Split('.');
            optional = true;
            foreach (var part in parts)
            {
                optional = schema["required"] != null && schema["required"].Contains(part);
                schema = (JObject)((JObject)schema["properties"])[part];
                if (part == null)
                {
                    throw new MissingFieldException($"{part} is not a property in your schema.");
                }
            }
            return schema;
        }

        protected Type ToType(JObject schema)
        {
            Type type = null;
            if (IsType(schema, "boolean")) type = typeof(bool);
            else if (IsType(schema, "integer")) type = typeof(long);
            else if (IsType(schema, "number")) type = typeof(double);
            else if (IsType(schema, "string")) type = typeof(string);
            else
            {
                throw new ArgumentException($"{schema} does not have a valid C# type.");
            }
            return type;
        }

        protected string[] Strings(JObject schema, string field)
        {
            string[] result = null;
            JToken array;
            if (schema.TryGetValue(field, out array))
            {
                result = array.ToObject<string[]>();
            }
            return result;
        }

        protected string AString(JObject schema, string field)
        {
            string result = null;
            JToken astring;
            if (schema.TryGetValue(field, out astring))
            {
                result = (string)astring;
            }
            return result;
        }

        protected void ProcessAnnotations(JObject schema, JObject fieldSchema, JObject eltSchema)
        {
            ProcessTemplates(schema);
            ProcessTemplates(fieldSchema);
            ProcessPrompt(fieldSchema);
            ProcessNumeric(fieldSchema);
        }

        protected void ProcessTemplates(JObject schema)
        {
            JToken templates;
            if (schema.TryGetValue("Templates", out templates))
            {
                foreach (JProperty template in templates.Children())
                {
                    TemplateUsage usage;
                    if (Enum.TryParse<TemplateUsage>(template.Name, out usage))
                    {
                        ReplaceTemplate((TemplateAttribute)ProcessTemplate(template.Value, new TemplateAttribute(usage)));
                    }
                }
            }
        }

        protected void ProcessPrompt(JObject schema)
        {
            JToken prompt;
            if (schema.TryGetValue("Prompt", out prompt))
            {
                SetPrompt((PromptAttribute)ProcessTemplate(prompt, new PromptAttribute()));
            }
        }

        protected void ProcessNumeric(JObject schema)
        {
            JToken token;
            double min = -double.MaxValue, max = double.MaxValue;
            if (schema.TryGetValue("minimum", out token)) min = (double)token;
            if (schema.TryGetValue("maximum", out token)) max = (double)token;
            if (min != -double.MaxValue || max != double.MaxValue)
            {
                SetLimits(min, max);
            }
        }

        protected void ProcessEnum(JObject schema)
        {
            if (schema["enum"] != null)
            {
                var enums = (from val in (JArray)schema["enum"] select (string)val);
                var toDescription = new Dictionary<string, string>();
                var toTerms = new Dictionary<string, string[]>();
                var toMaxPhrase = new Dictionary<string, int>();
                JToken values;
                if (schema.TryGetValue("Values", out values))
                {
                    foreach (JProperty prop in values.Children())
                    {
                        var key = prop.Name;
                        if (!enums.Contains(key))
                        {
                            throw new ArgumentException($"{key} is not in enumeration.");
                        }
                        var desc = (JObject)prop.Value;
                        JToken description;
                        if (desc.TryGetValue("Describe", out description))
                        {
                            toDescription.Add(key, (string)description);
                        }
                        JToken terms;
                        if (desc.TryGetValue("Terms", out terms))
                        {
                            toTerms.Add(key, terms.ToObject<string[]>());
                        }
                        JToken maxPhrase;
                        if (desc.TryGetValue("MaxPhrase", out maxPhrase))
                        {
                            toMaxPhrase.Add(key, (int)maxPhrase);
                        }
                    }
                }
                foreach (var key in enums)
                {
                    string description;
                    if (!toDescription.TryGetValue(key, out description))
                    {
                        description = Language.CamelCase(key);
                    }
                    AddDescription(key, description);

                    string[] terms;
                    int maxPhrase;
                    if (!toTerms.TryGetValue(key, out terms))
                    {
                        terms = Language.GenerateTerms(description, 3);
                    }
                    else if (toMaxPhrase.TryGetValue(key, out maxPhrase))
                    {
                        terms = (from seed in terms
                                 from gen in Language.GenerateTerms(seed, maxPhrase)
                                 select gen).ToArray<string>();
                    }
                    AddTerms(key, terms);
                }
            }
        }

        protected TemplateBaseAttribute ProcessTemplate(JToken template, TemplateBaseAttribute attribute)
        {
            attribute.Patterns = template["Patterns"].ToObject<string[]>();
            attribute.AllowDefault = ProcessEnum<BoolDefault>(template, "AllowDefault");
            attribute.ChoiceCase = ProcessEnum<CaseNormalization>(template, "ChoiceCase");
            attribute.ChoiceFormat = (string)template["ChoiceFormat"];
            attribute.ChoiceLastSeparator = (string)template["ChoiceLastSeparator"];
            attribute.ChoiceParens = ProcessEnum<BoolDefault>(template, "ChoiceParens");
            attribute.ChoiceSeparator = (string)template["ChoiceSeparator"];
            attribute.ChoiceStyle = ProcessEnum<ChoiceStyleOptions>(template, "ChoiceStyle");
            attribute.Feedback = ProcessEnum<FeedbackOptions>(template, "Feedback");
            attribute.FieldCase = ProcessEnum<CaseNormalization>(template, "FieldCase");
            attribute.LastSeparator = (string)template["LastSeparator"];
            attribute.Separator = (string)template["Separator"];
            attribute.ValueCase = ProcessEnum<CaseNormalization>(template, "ValueCase");
            return attribute;
        }

        protected T ProcessEnum<T>(JToken template, string name)
        {
            T result = default(T);
            var value = template[name];
            if (value != null)
            {
                result = (T)Enum.Parse(typeof(T), (string)value);
            }
            return result;
        }


        internal static bool IsType(JObject schema, string type)
        {
            bool isType = false;
            var jtype = schema["type"];
            if (jtype != null)
            {
                if (jtype is JArray)
                {
                    isType = jtype.Values().Contains(type);
                }
                else
                {
                    isType = (string)jtype == type;
                }
            }
            return isType;
        }

        internal static bool IsPrimitiveType(JObject schema)
        {
            var isPrimitive = schema["enum"] != null && schema["enum"].Any();
            if (!isPrimitive)
            {
                isPrimitive =
                    IsType(schema, "boolean")
                    || IsType(schema, "integer")
                    || IsType(schema, "number")
                    || IsType(schema, "string")
                    || (schema["DateTime"] != null && (bool)schema["DateTime"]);
            }
            return isPrimitive;
        }

        internal static JObject ElementSchema(JObject schema)
        {
            JObject result = schema;
            if (IsType(schema, "array"))
            {
                var items = schema["items"];
                if (items is JArray)
                {
                    result = (JObject)((JArray)items).First();
                }
                else
                {
                    result = (JObject)items;
                }
            }
            return result;
        }

        protected JObject _schema;
    }
}

namespace Microsoft.Bot.Builder.FormFlow
{
    public static partial class Extensions
    {
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
        /// Add any remaining fields defined in <paramref name="schema"/>.
        /// </summary>
        /// <param name="builder">Where to add any defined fields.</param>
        /// <param name="schema">JSON Schema that defines fields.</param>
        /// <param name="exclude">Fields not to include.</param>
        /// <returns>Modified <see cref="IFormBuilder{T}"/>.</returns>
        /// <remarks>
        /// See <see cref="FieldJson"/> for a description of JSON Schema extensions 
        /// for defining your fields.
        /// </remarks>
        public static IFormBuilder<JObject> AddRemainingFields(this IFormBuilder<JObject> builder, JObject schema, IEnumerable<string> exclude = null)
        {
            var exclusions = (exclude == null ? Array.Empty<string>() : exclude.ToArray());
            var fields = new List<string>();
            Fields(schema, null, fields);
            foreach (var field in fields)
            {
                if (!exclusions.Contains(field) && !builder.HasField(field))
                {
                    builder.Field(schema, field);
                }
            }
            return builder;
        }

        /// <summary>
        /// Define a step for filling in a particular value in a JObject as defined by a JSON Schema.
        /// </summary>
        /// <param name="builder">Form builder.</param>
        /// <param name="schema">JSON schema defining JObject.</param>
        /// <param name="name">Path in the form state to the value being filled in.</param>
        /// <param name="active">Delegate to test form state to see if step is active.</param>
        /// <param name="validate">Delegate to validate the field value.</param>
        /// <returns>This form.</returns>
        /// <remarks>
        /// See <see cref="FieldJson"/> for a description of JSON Schema extensions 
        /// for defining your fields.
        /// </remarks>
        public static IFormBuilder<JObject> Field(this IFormBuilder<JObject> builder, JObject schema, string name, ActiveDelegate<JObject> active = null, ValidateAsyncDelegate<JObject> validate = null)
        {
            var field = new FieldJson(schema, name);
            if (active != null)
            {
                field.SetActive(active);
            }
            if (validate != null)
            {
                field.SetValidate(validate);
            }
            return builder.Field(field);
        }

        /// <summary>
        /// Define a step for filling in a particular value in a JObject as defined by a JSON Schema.
        /// </summary>
        /// <param name="builder">Form builder.</param>
        /// <param name="schema">JSON schema defining JObject.</param>
        /// <param name="name">Path in the form state to the value being filled in.</param>
        /// <param name="prompt">Simple \ref patterns to describe prompt for field.</param>
        /// <param name="active">Delegate to test form state to see if step is active.n</param>
        /// <param name="validate">Delegate to validate the field value.</param>
        /// <returns>This form.</returns>
        /// <remarks>
        /// See <see cref="FieldJson"/> for a description of JSON Schema extensions 
        /// for defining your fields.
        /// </remarks>
        public static IFormBuilder<JObject> Field(this IFormBuilder<JObject> builder, JObject schema, string name, string prompt, ActiveDelegate<JObject> active = null, ValidateAsyncDelegate<JObject> validate = null)
        {
            return builder.Field(schema, name, new PromptAttribute(prompt), active, validate);
        }

        /// <summary>
        /// Define a step for filling in a particular value in a JObject as defined by a JSON Schema.
        /// </summary>
        /// <param name="builder">Form builder.</param>
        /// <param name="schema">JSON schema defining JObject.</param>
        /// <param name="name">Path in the form state to the value being filled in.</param>
        /// <param name="prompt">Prompt pattern with more formatting control to describe prompt for field.</param>
        /// <param name="active">Delegate to test form state to see if step is active.n</param>
        /// <param name="validate">Delegate to validate the field value.</param>
        /// <returns>This form.</returns>
        /// <remarks>
        /// See <see cref="FieldJson"/> for a description of JSON Schema extensions 
        /// for defining your fields.
        /// </remarks>
        public static IFormBuilder<JObject> Field(this IFormBuilder<JObject> builder, JObject schema, string name, PromptAttribute prompt, ActiveDelegate<JObject> active = null, ValidateAsyncDelegate<JObject> validate = null)
        {
            var field = new FieldJson(schema, name);
            field.SetPrompt(prompt);
            if (active != null)
            {
                field.SetActive(active);
            }
            if (validate != null)
            {
                field.SetValidate(validate);
            }
            return builder.Field(field);
        }
    }
}