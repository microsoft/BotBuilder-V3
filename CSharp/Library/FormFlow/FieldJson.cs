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

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.FormFlow.Advanced
{
    /// <summary>
    /// State argument for JSon FormFlow scripts.
    /// </summary>
    public class ScriptState
    {
        /// <summary>
        /// Current state for JSON FormFlow scripts. 
        /// </summary>
        /// <remarks>Passed as a dynamic to make usage simpler, but is actually a JObject.</remarks>
        public dynamic state;
    }

    /// <summary>
    /// Arguments for Validate scripts in JSON FormFlow.
    /// </summary>
    public class ScriptValidate : ScriptState
    {
        /// <summary>
        ///  Value to validate.
        /// </summary>
        public object value;
    }

    /// <summary>
    /// Arguments for Define scripts in JSON FormFlow.
    /// </summary>
    public class ScriptField : ScriptState
    {
        /// <summary>
        /// Field being dynamically defined.
        /// </summary>
        public Field<JObject> field;
    }

    /// <summary>
    /// Arguments for OnCompletion scripts in JSON FormFlow.
    /// </summary>
    public class ScriptOnCompletion : ScriptState
    {
        /// <summary>
        /// Bot Framework context.
        /// </summary>
        public IDialogContext context;
    }

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
    /// * `pattern` -- For string fields will be used to validate the entered pattern as described in <see cref="PatternAttribute"/>.
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
    /// * `OnCompletion: script` -- C# script with arguments (IDialogContext context, JObject state) for completing form.
    /// * `References: [assemblyReference, ...]` -- Define references to include in scripts.  Paths should be absolute, or relative to the current directory.  By default Microsoft.Bot.Builder.dll is included and there is a using Microsoft.Bot.Builder.FormFlow.
    /// 
    /// %Extensions that are found in a property description as peers to the "type" property of a JSON Schema.
    /// * `DateTime:bool` -- Marks a field as being a DateTime field.
    /// * `Describe:string` -- Description of a field.
    /// * `Terms:[string,...]` -- Regular expressions for matching a field value.
    /// * `MaxPhrase:int` -- This will run your terms through <see cref="Language.GenerateTerms(string, int)"/> to expand them.
    /// * `Values:{ string: {Describe:string, Terms:[string, ...], MaxPhrase}, ...}` -- The string must be found in the types "enum" and this allows you to override the automatically generated descriptions and terms.  If MaxPhrase is specified the terms are passed through <see cref="Language.GenerateTerms(string, int)"/>.
    /// * `Active:script` -- C# script with arguments (dynamic state)->bool to test to see if field/message/confirm is active.
    /// * `Validate:script` -- C# script with arguments (dynamic state, object value)->ValidateResult for validating a field value.
    /// * `Define:script` -- C# script with arguments (dynamic state, Field field) for dynamically defining a field.  
    /// * `Before:[confirm|message, ...]` -- Messages or confirmations before the containing field.
    /// * `After:[confirm|message, ...]` -- Messages or confirmations after the containing field.
    /// * `{Confirm:script|[string, ...], ...templateArgs}` -- With Before/After define a confirmation through either C# script with argument (dynamic state) or through a set of patterns that will be randomly selected with optional template arguments.
    /// * `{Message:script|[string, ...] ...templateArgs}` -- With Before/After define a message through either C# script with argument (dynamic state) or through a set of patterns that will be randomly selected with optional template arguments.
    /// * `Dependencies`:[string, ...]` -- Fields that this field, message or confirm depends on.
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

        internal IEnumerable<MessageOrConfirmation> Before { get; set; }

        internal IEnumerable<MessageOrConfirmation> After { get; set; }

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
            ProcessReferences(schema);
            ProcessTemplates(schema);
            Before = ProcessMessages("Before", fieldSchema);
            ProcessTemplates(fieldSchema);
            ProcessPrompt(fieldSchema);
            ProcessNumeric(fieldSchema);
            ProcessPattern(fieldSchema);
            ProcessActive(fieldSchema);
            ProcessDefine(fieldSchema);
            ProcessValidation(fieldSchema);
            After = ProcessMessages("After", fieldSchema);
        }

        protected void ProcessDefine(JObject schema)
        {
            if (schema["Define"] != null)
            {
                var script = (string)schema["Define"];
                SetDefine(async (state, field) => await EvaluateAsync<bool>(script, new ScriptField { state = state, field = field }));
            }
        }

        // NOTE: This is not being used because every invocation creates an assembly that cannot be
        // garbage collected whereas EvaluateAsync does not.
        protected ScriptRunner<R> Compile<G, R>(string code)
        {
            try
            {
                var script = CSharpScript.Create<R>(code, _options, typeof(G));
                return script.CreateDelegate();
            }
            catch (Microsoft.CodeAnalysis.Scripting.CompilationErrorException ex)
            {
                throw CompileException(ex, code);
            }
        }

        protected async Task<T> EvaluateAsync<T>(string code, object globals)
        {
            try
            {
                var script = CSharpScript.Create<T>(code, _options, globals.GetType());
                var fun = script.CreateDelegate();

                var timer = System.Diagnostics.Stopwatch.StartNew();
                var result = await fun(globals);
                // var result = await CSharpScript.EvaluateAsync<T>(code, _options, globals);
                System.Diagnostics.Debug.Write($"Eval took {timer.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Microsoft.CodeAnalysis.Scripting.CompilationErrorException ex)
            {
                throw CompileException(ex, code);
            }
        }

        protected Exception CompileException(CompilationErrorException ex, string code)
        {
            Exception result = ex;
            var match = System.Text.RegularExpressions.Regex.Match(ex.Message, @"\(\s*(?<line>\d+)\s*,\s*(?<column>\d+)\s*\)\s*:\s*(?<message>.*)");
            if (match.Success)
            {
                var lineNumber = int.Parse(match.Groups["line"].Value) - 1;
                var column = int.Parse(match.Groups["column"].Value) - 1;
                var line = code.Split('\n')[lineNumber];
                var minCol = Math.Max(0, column - 20);
                var maxCol = Math.Min(line.Length, column + 20);
                var msg = line.Substring(minCol, column - minCol) + "^" + line.Substring(column, maxCol - column);
                result = new ArgumentException(match.Groups["message"].Value + ": " + msg);
            }
            return result;
        }

        protected void ProcessValidation(JObject schema)
        {
            if (schema["Validate"] != null)
            {
                var script = (string)schema["Validate"];
                SetValidate(async (state, value) => await EvaluateAsync<ValidateResult>(script, new ScriptValidate { state = state, value = value }));
            }
        }

        protected void ProcessActive(JObject schema)
        {
            if (schema["Active"] != null)
            {
                var script = (string)schema["Active"];
                SetActive((state) => EvaluateAsync<bool>(script, new ScriptState { state = state }).Result);
            }
        }

        internal class MessageOrConfirmation
        {
            public bool IsMessage;
            public PromptAttribute Prompt;
            public ActiveDelegate<JObject> Condition;
            public MessageDelegate<JObject> MessageGenerator;
            public IEnumerable<string> Dependencies;
        }

        internal IEnumerable<MessageOrConfirmation> ProcessMessages(string fieldName, JObject fieldSchema)
        {
            var messages = new List<MessageOrConfirmation>();
            JToken array;
            if (fieldSchema.TryGetValue(fieldName, out array))
            {
                foreach (var message in array.Children<JObject>())
                {
                    var info = new MessageOrConfirmation();
                    if (GetPrompt("Message", message, info))
                    {
                        info.IsMessage = true;
                        messages.Add(info);
                    }
                    else if (GetPrompt("Confirm", message, info))
                    {
                        info.IsMessage = false;
                        messages.Add(info);
                    }
                    else
                    {
                        throw new ArgumentException($"{message} is not Message or Confirm");
                    }
                }
            }
            return messages;
        }

        internal bool GetPrompt(string fieldName, JObject message, MessageOrConfirmation info)
        {
            bool found = false;
            JToken val;
            if (message.TryGetValue(fieldName, out val))
            {
                if (val is JValue)
                {
                    var script = (string)val;
                    info.MessageGenerator = async (state) => await EvaluateAsync<PromptAttribute>(script, new ScriptState { state = state });
                }
                else if (val is JArray)
                {
                    info.Prompt = (PromptAttribute)ProcessTemplate(message, new PromptAttribute((from msg in val select (string)msg).ToArray()));
                }
                else
                {
                    throw new ArgumentException($"{val} must be string or array of strings.");
                }
                if (message["Active"] != null)
                {
                    var script = (string)message["Active"];
                    info.Condition = (state) => EvaluateAsync<bool>(script, new ScriptState { state = state }).Result;
                }
                if (message["Dependencies"] != null)
                {
                    info.Dependencies = (from dependent in message["Dependencies"] select (string)dependent);
                }
                found = true;
            }
            return found;
        }

        protected void ProcessReferences(JObject schema)
        {
            JToken references;
            var assemblies = new List<string>() { "Microsoft.Bot.Builder.dll" };
            if (schema.TryGetValue("References", out references))
            {
                foreach (JToken template in references.Children())
                {
                    assemblies.Add((string)template);
                }
            }
            var dir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);
            _options = CodeAnalysis.Scripting.ScriptOptions.Default
                .AddReferences((from assembly in assemblies select System.IO.Path.Combine(dir, assembly)).ToArray())
                .AddImports("Microsoft.Bot.Builder.FormFlow", "Microsoft.Bot.Builder.FormFlow.Advanced", "System.Collections.Generic", "System.Linq", "System");
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

        protected void ProcessPattern(JObject schema)
        {
            JToken token;
            if (schema.TryGetValue("pattern", out token))
            {
                SetPattern((string)token);
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
            if (template["Patterns"] != null)
            {
                attribute.Patterns = template["Patterns"].ToObject<string[]>();
            }
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
        protected CodeAnalysis.Scripting.ScriptOptions _options;
    }
}
