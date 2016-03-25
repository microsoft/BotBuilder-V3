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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Form.Advanced
{
    /// <summary>Base class with declarative implementation of <see cref="IField{T}"/>. </summary>
    /// <typeparam name="T">Underlying form state.</typeparam>
    public class Field<T> : IField<T>
    {
        /// <summary>   Construct field. </summary>
        /// <param name="name"> Name of field. </param>
        /// <param name="role"> Role field plays in form. </param>
        /// <param name="form"> The form. </param>
        public Field(string name, FieldRole role, IForm<T> form)
        {
            _name = name;
            _role = role;
            _form = form;

            foreach (var template in form.Configuration.Templates)
            {
                if (!_templates.ContainsKey(template.Usage))
                {
                    AddTemplate(template);
                }
            }
            if (_help == null)
            {
                var template = Template(TemplateUsage.Help);
                _help = new Prompt(template);
            }
        }

        #region IField

        public string Name { get { return _name; } }

        public IForm<T> Form { get { return this._form; } }

        #region IFieldState
        public virtual object GetValue(T state)
        {
            throw new NotImplementedException();
        }

        public virtual void SetValue(T state, object value)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsUnknown(T state)
        {
            throw new NotImplementedException();
        }

        public virtual void SetUnknown(T state)
        {
            throw new NotImplementedException();
        }

        public virtual bool Optional()
        {
            return _optional;
        }

        public virtual bool IsNullable()
        {
            return _isNullable;
        }

        public virtual bool Limits(out double min, out double max)
        {
            min = _min;
            max = _max;
            return _limited;
        }

        public virtual IEnumerable<string> Dependencies()
        {
            return new string[0];
        }
        #endregion

        #region IFieldDescription
        public virtual FieldRole Role()
        {
            return _role;
        }

        public virtual string Description()
        {
            return _description;
        }

        public virtual IEnumerable<string> Terms()
        {
            return _terms;
        }

        public virtual IEnumerable<string> Terms(object value)
        {
            return _valueTerms[value];
        }

        public virtual string ValueDescription(object value)
        {
            return _valueDescriptions[value];
        }

        public virtual IEnumerable<string> ValueDescriptions()
        {
            return (from entry in _valueDescriptions select entry.Value);
        }

        public virtual IEnumerable<object> Values()
        {
            return (from entry in _valueDescriptions select entry.Key);
        }

        public virtual bool AllowsMultiple()
        {
            return _allowsMultiple;
        }

        public virtual bool AllowDefault()
        {
            return _promptDefinition.AllowDefault != BoolDefault.False;
        }

        public bool AllowNumbers()
        {
            _promptDefinition.ApplyDefaults(_form.Configuration.DefaultPrompt);
            return _promptDefinition.AllowNumbers;
        }

        #endregion

        #region IFieldPrompt

        public virtual bool Active(T state)
        {
            return true;
        }

        public virtual Template Template(TemplateUsage usage)
        {
            Template template;
            _templates.TryGetValue(usage, out template);
            if (template != null)
            {
                template.ApplyDefaults(_form.Configuration.DefaultPrompt);
            }
            return template;
        }

        public virtual IPrompt<T> Prompt()
        {
            if (_prompt == null)
            {
                _prompt = new Prompter<T>(_promptDefinition, _form, _recognizer);
            }
            return _prompt;
        }

        public virtual Task<string> ValidateAsync(T state, object value)
        {
            return _validate(state, value);
        }

        public virtual IPrompt<T> Help()
        {
            return new Prompter<T>(_help, _form, Prompt().Recognizer());
        }

        public virtual NextStep Next(object value, T state)
        {
            return new NextStep();
        }

        #endregion
        #endregion

        #region Publics
        /// <summary>Set the field description. </summary>
        /// <param name="description">Field description. </param>
        /// <returns>   A Field&lt;T&gt; </returns>
        public Field<T> Description(string description)
        {
            UpdateAnnotations();
            _description = description;
            return this;
        }

        /// <summary>   Set the help associated with a field. </summary>
        /// <param name="help"> Help string. </param>
        /// <returns>   A Field&lt;T&gt; </returns>
        public Field<T> Help(Prompt help)
        {
            UpdateAnnotations();
            _help = help;
            return this;
        }

        /// <summary>   Set the terms associated with the field. </summary>
        /// <param name="terms">    The terms. </param>
        /// <returns>   A Field&lt;T&gt; </returns>
        public Field<T> Terms(IEnumerable<string> terms)
        {
            UpdateAnnotations();
            _terms = terms.ToArray();
            return this;
        }

        /// <summary>   Adds a description for a value. </summary>
        /// <param name="value">        The value. </param>
        /// <param name="description">  Description of the value. </param>
        /// <returns>   A Field&lt;T&gt; </returns>
        public Field<T> AddDescription(object value, string description)
        {
            UpdateAnnotations();
            _valueDescriptions[value] = description;
            return this;
        }

        /// <summary>   Adds terms for a value. </summary>
        /// <param name="value">    The value. </param>
        /// <param name="terms">    The terms. </param>
        /// <returns>   A Field&lt;T&gt; </returns>
        public Field<T> AddTerms(object value, IEnumerable<string> terms)
        {
            UpdateAnnotations();
            _valueTerms[value] = terms.ToArray();
            return this;
        }

        /// <summary>   Removes the description and terms associated with a value. </summary>
        /// <param name="value">    The value to remove. </param>
        /// <returns>   A Field&lt;T&gt; </returns>
        public Field<T> RemoveValue(object value)
        {
            UpdateAnnotations();
            _valueDescriptions.Remove(value);
            _valueTerms.Remove(value);
            return this;
        }

		/// <summary>   Set whether or not a field is optional. </summary>
        /// <param name="optional"> True if field is optional. </param>
        /// <returns>   A Field&lt;T&gt; </returns>
        public Field<T> Optional(bool optional = true)
        {
            UpdateAnnotations();
            _optional = optional;
            return this;
        }

        /// <summary>   Set whether or not field is nullable. </summary>
        /// <param name="nullable"> True if field is nullable. </param>
        /// <returns>   A Field&lt;T&gt; </returns>
        public Field<T> IsNullable(bool nullable = true)
        {
            UpdateAnnotations();
            _isNullable = nullable;
            return this;
        }

        /// <summary>   Sets the field prompt. </summary>
        /// <param name="prompt">   The prompt. </param>
        /// <returns>   A Field&lt;T&gt; </returns>
        public Field<T> Prompt(Prompt prompt)
        {
            UpdateAnnotations();
            _promptDefinition = prompt;
            return this;
        }

        /// <summary> Sets the recognizer for the field. </summary>
        /// <param name="recognizer">   The recognizer for the field. </param>
        /// <returns>   A Field&lt;T&gt; </returns>
        public Field<T> Recognizer(IRecognize<T> recognizer)
        {
            UpdateAnnotations();
            _recognizer = recognizer;
            return this;
        }

        /// <summary>   Add a template to the field. </summary>
        /// <param name="template"> The template. </param>
        /// <returns>   A Field&lt;T&gt; </returns>
        public Field<T> Template(Template template)
        {
            UpdateAnnotations();
            AddTemplate(template);
            return this;
        }

        /// <summary>   Set the field validation. </summary>
        /// <param name="validate"> The validator. </param>
        /// <returns>   An IField&lt;T&gt; </returns>
        public IField<T> Validate(ValidateDelegate<T> validate)
        {
            UpdateAnnotations();
            _validate = validate;
            return this;
        }

        /// <summary>   Set numeric limits. </summary>
        /// <param name="min">  The minimum. </param>
        /// <param name="max">  The maximum. </param>
        /// <returns>   An IField&lt;T&gt; </returns>
        public IField<T> Numeric(double min, double max)
        {
            UpdateAnnotations();
            SetLimits(min, max, true);
            return this;
        }
        #endregion

        #region Internals
        protected void SetLimits(double min, double max, bool limited)
        {
            _min = min;
            _max = max;
            _limited = limited;
        }

        protected void UpdateAnnotations()
        {
            _prompt = null;
        }

        protected void AddTemplate(Template template)
        {
            _templates[template.Usage] = template;
        }

        protected IForm<T> _form;
        protected string _name;
        protected FieldRole _role;
        protected double _min, _max;
        protected bool _limited;
        protected bool _allowsMultiple;
        protected bool _optional;
        protected bool _isNullable;
        protected bool _keepZero;
        protected string _description;
        protected Prompt _help;
        protected ValidateDelegate<T> _validate = new ValidateDelegate<T>(async (state, value) => null);
        protected string[] _terms = new string[0];
        protected Dictionary<object, string> _valueDescriptions = new Dictionary<object, string>();
        protected Dictionary<object, string[]> _valueTerms = new Dictionary<object, string[]>();
        protected Dictionary<TemplateUsage, Template> _templates = new Dictionary<TemplateUsage, Template>();
        protected Prompt _promptDefinition;
        protected IRecognize<T> _recognizer;
        protected IPrompt<T> _prompt;
        #endregion
    }

    public class FieldReflector<T> : Field<T>
        where T : class, new()
    {
        public FieldReflector(string name, IForm<T> form, bool ignoreAnnotations = false)
            : base(name, FieldRole.Value, form)
        {
            _ignoreAnnotations = ignoreAnnotations;
            AddField(typeof(T), string.IsNullOrWhiteSpace(_name) ? new string[] { } : _name.Split('.'), 0);

            if (_promptDefinition == null)
            {
                if (_type.IsEnum)
                {
                    _promptDefinition = new Prompt(Template(_allowsMultiple ? TemplateUsage.EnumSelectMany : TemplateUsage.EnumSelectOne));
                }
                else if (_type == typeof(string))
                {
                    _promptDefinition = new Prompt(Template(TemplateUsage.String));
                }
                else if (_type.IsIntegral())
                {
                    _promptDefinition = new Prompt(Template(TemplateUsage.Integer));
                }
                else if (_type == typeof(bool))
                {
                    _promptDefinition = new Prompt(Template(TemplateUsage.Bool));
                }
                else if (_type.IsDouble())
                {
                    _promptDefinition = new Prompt(Template(TemplateUsage.Double));
                }
                else if (_type == typeof(DateTime))
                {
                    _promptDefinition = new Prompt(Template(TemplateUsage.DateTime));
                }
            }

            var step = _path.LastOrDefault();
            if (_type == null || _type.IsEnum)
            {
                _recognizer = new RecognizeEnumeration<T>(this);
            }
            else if (_type == typeof(bool))
            {
                _recognizer = new RecognizeBool<T>(this);
            }
            else if (_type == typeof(string))
            {
                _recognizer = new RecognizeString<T>(this);
            }
            else if (_type.IsIntegral())
            {
                _recognizer = new RecognizeNumber<T>(this, CultureInfo.CurrentCulture);
            }
            else if (_type.IsDouble())
            {
                _recognizer = new RecognizeDouble<T>(this, CultureInfo.CurrentCulture);
            }
            else if (_type == typeof(DateTime))
            {
                _recognizer = new RecognizeDateTime<T>(this, CultureInfo.CurrentCulture);
            }
            else if (_type.IsIEnumerable())
            {
                var elt = _type.GetGenericElementType();
                if (elt.IsEnum)
                {
                    _recognizer = new RecognizeEnumeration<T>(this);
                }
            }
        }

        #region IField

        #region IFieldState
        public override object GetValue(T state)
        {
            object current = state;
            bool isEnum = false;
            foreach (var step in _path)
            {
                var field = step as FieldInfo;
                var ftype = StepType(step);
                if (field != null)
                {
                    current = field.GetValue(current);
                }
                else
                {
                    var prop = step as PropertyInfo;
                    current = prop.GetValue(current);
                }
                if (current == null)
                {
                    break;
                }
                isEnum = ftype.IsEnum;
            }
            return isEnum ? ((int)current == 0 ? null : current) : current;
        }

        public override void SetValue(T state, object value)
        {
            object current = state;
            object lastClass = state;
            var last = _path.Last();
            foreach (var step in _path)
            {
                var field = step as FieldInfo;
                var prop = step as PropertyInfo;
                Type ftype = StepType(step);
                if (step == last)
                {
                    object newValue = value;
                    if (ftype.IsIEnumerable())
                    {
                        if (value != null && ftype != typeof(string))
                        {
                            // Build list and coerce elements
                            var list = Activator.CreateInstance(ftype);
                            var addMethod = list.GetType().GetMethod("Add");
                            foreach (var elt in value as System.Collections.IEnumerable)
                            {
                                addMethod.Invoke(list, new object[] { elt });
                            }
                            newValue = list;
                        }
                    }
                    else
                    {
                        if (value == null && (ftype.IsEnum || ftype.IsIntegral() || ftype.IsDouble()))
                        {
                            // Default value for numbers and enums
                            newValue = 0;
                        }
                        else if (ftype.IsIntegral())
                        {
                            newValue = Convert.ChangeType(value, ftype);
                        }
                        else if (ftype.IsDouble())
                        {
                            newValue = Convert.ChangeType(value, ftype);
                        }
                        else if (ftype == typeof(bool))
                        {
                            newValue = Convert.ChangeType(value, typeof(bool));
                        }
                    }
                    if (field != null)
                    {
                        field.SetValue(lastClass, newValue);
                    }
                    else
                    {
                        prop.SetValue(lastClass, newValue);
                    }
                }
                else
                {
                    current = (field == null ? prop.GetValue(current) : field.GetValue(current));
                    if (current == null)
                    {
                        var obj = Activator.CreateInstance(ftype);
                        current = obj;
                        if (field != null)
                        {
                            field.SetValue(lastClass, current);
                        }
                        else
                        {
                            prop.SetValue(lastClass, current);
                        }
                    }
                    lastClass = current;
                }
            }
        }

        public override bool IsUnknown(T state)
        {
            var unknown = false;
            var value = GetValue(state);
            if (value == null)
            {
                unknown = true;
            }
            else
            {
                var step = _path.Last();
                var ftype = StepType(step);
                if (ftype.IsValueType && ftype.IsEnum)
                {
                    unknown = ((int)value == 0);
                }
                else if (ftype == typeof(DateTime))
                {
                    unknown = ((DateTime)value) == default(DateTime);
                }
                else if (ftype.IsIEnumerable())
                {
                    unknown = !(value as System.Collections.IEnumerable).GetEnumerator().MoveNext();
                }
            }
            return unknown;
        }

        public override void SetUnknown(T state)
        {
            var step = _path.Last();
            var field = step as FieldInfo;
            var prop = step as PropertyInfo;
            var ftype = StepType(step);
            if (ftype.IsEnum)
            {
                SetValue(state, 0);
            }
            else if (ftype == typeof(DateTime))
            {
                SetValue(state, default(DateTime));
            }
            else
            {
                SetValue(state, null);
            }
        }

        #endregion

        #endregion

        #region Internals
        protected Type StepType(object step)
        {
            var field = step as FieldInfo;
            var prop = step as PropertyInfo;
            return (step == null ? null : (field == null ? prop.PropertyType : field.FieldType));
        }

        protected void AddField(Type type, string[] path, int ipath)
        {
            if (ipath < path.Length)
            {
                ProcessTemplates(type);
                var step = path[ipath];
                object field = type.GetField(step, BindingFlags.Public | BindingFlags.Instance);
                Type ftype;
                if (field == null)
                {
                    var prop = type.GetProperty(step, BindingFlags.Public | BindingFlags.Instance);
                    if (prop == null)
                    {
                        throw new ArgumentException(step + " is not a field or property in your type");
                    }
                    field = prop;
                    ftype = prop.PropertyType;
                    _path.Add(prop);
                }
                else
                {
                    ftype = (field as FieldInfo).FieldType;
                    _path.Add(field);
                }
                if (ftype.IsNullable())
                {
                    _isNullable = true;
                    _keepZero = true;
                    ftype = Nullable.GetUnderlyingType(ftype);
                }
                else if (ftype.IsEnum || ftype.IsClass)
                {
                    _isNullable = true;
                }
                if (ftype.IsClass)
                {
                    if (ftype == typeof(string))
                    {
                        _type = ftype;
                        ProcessFieldAttributes(field);
                    }
                    else if (ftype.IsIEnumerable())
                    {
                        var elt = ftype.GetGenericElementType();
                        if (elt.IsEnum)
                        {
                            _type = elt;
                            _allowsMultiple = true;
                            ProcessFieldAttributes(field);
                            ProcessEnumAttributes(elt);
                        }
                        else
                        {
                            // TODO: What to do about enumerations of things other than enums?
                            throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        AddField(ftype, path, ipath + 1);
                    }
                }
                else
                {
                    if (ftype.IsEnum)
                    {
                        ProcessFieldAttributes(field);
                        ProcessEnumAttributes(ftype);
                    }
                    else if (ftype == typeof(bool))
                    {
                        ProcessFieldAttributes(field);
                    }
                    else if (ftype.IsIntegral())
                    {
                        long min = long.MinValue;
                        long max = long.MaxValue;
                        if (ftype == typeof(sbyte)) { min = sbyte.MinValue; max = sbyte.MaxValue; }
                        else if (ftype == typeof(byte)) { min = byte.MinValue; max = byte.MaxValue; }
                        else if (ftype == typeof(short)) { min = short.MinValue; max = short.MaxValue; }
                        else if (ftype == typeof(ushort)) { min = ushort.MinValue; max = ushort.MaxValue; }
                        else if (ftype == typeof(int)) { min = int.MinValue; max = int.MaxValue; }
                        else if (ftype == typeof(uint)) { min = uint.MinValue; max = uint.MaxValue; }
                        else if (ftype == typeof(long)) { min = long.MinValue; max = long.MaxValue; }
                        else if (ftype == typeof(ulong)) { min = long.MinValue; max = long.MaxValue; }
                        SetLimits(min, max, false);
                        ProcessFieldAttributes(field);
                    }
                    else if (ftype.IsDouble())
                    {
                        double min = long.MinValue;
                        double max = long.MaxValue;
                        if (ftype == typeof(float)) { min = float.MinValue; max = float.MaxValue; }
                        else if (ftype == typeof(double)) { min = double.MinValue; max = double.MaxValue; }
                        SetLimits(min, max, false);
                        ProcessFieldAttributes(field);
                    }
                    else if (ftype == typeof(DateTime))
                    {
                        ProcessFieldAttributes(field);
                    }
                    _type = ftype;
                }
            }
        }

        protected void ProcessTemplates(Type type)
        {
            if (!_ignoreAnnotations)
            {
                foreach (var attribute in type.GetCustomAttributes(typeof(Template)))
                {
                    AddTemplate(attribute as Template);
                }
            }
        }

        protected void ProcessFieldAttributes(object step)
        {
            _optional = false;
            if (!_ignoreAnnotations)
            {
                var field = step as FieldInfo;
                var prop = step as PropertyInfo;
                var name = (field == null ? prop.Name : field.Name);
                var describe = (field == null ? prop.GetCustomAttribute<Describe>() : field.GetCustomAttribute<Describe>());
                var terms = (field == null ? prop.GetCustomAttribute<Terms>() : field.GetCustomAttribute<Terms>());
                var prompt = (field == null ? prop.GetCustomAttribute<Prompt>() : field.GetCustomAttribute<Prompt>());
                var optional = (field == null ? prop.GetCustomAttribute<Optional>() : field.GetCustomAttribute<Optional>());
                var numeric = (field == null ? prop.GetCustomAttribute<Numeric>() : field.GetCustomAttribute<Numeric>());
                if (describe != null)
                {
                    _description = describe.Description;
                }
                else
                {
                    _description = Language.CamelCase(name);
                }
                if (terms != null)
                {
                    _terms = terms.Alternatives;
                }
                else
                {
                    _terms = Language.GenerateTerms(Language.CamelCase(name), 3);
                }
                if (prompt != null)
                {
                    _promptDefinition = prompt;
                }
                if (numeric != null)
                {
                    double oldMin, oldMax;
                    Limits(out oldMin, out oldMax);
                    SetLimits(numeric.Min, numeric.Max, numeric.Min != oldMin || numeric.Max != oldMax);
                }
                _optional = (optional != null);
                foreach (var attribute in (field == null ? prop.GetCustomAttributes<Template>() : field.GetCustomAttributes<Template>()))
                {
                    AddTemplate(attribute as Template);
                }
            }
        }

        protected void ProcessEnumAttributes(Type type)
        {
            foreach (var enumField in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var enumValue = enumField.GetValue(null);
                if (_keepZero || (int)enumValue > 0)
                {
                    var describe = enumField.GetCustomAttribute<Describe>();
                    var terms = enumField.GetCustomAttribute<Terms>();
                    if (describe != null && !_ignoreAnnotations)
                    {
                        _valueDescriptions.Add(enumValue, describe.Description);
                    }
                    else
                    {
                        _valueDescriptions.Add(enumValue, Language.CamelCase(enumValue.ToString()));
                    }
                    if (terms != null && !_ignoreAnnotations)
                    {
                        _valueTerms.Add(enumValue, terms.Alternatives);
                    }
                    else
                    {
                        _valueTerms.Add(enumValue, Language.GenerateTerms(Language.CamelCase(enumValue.ToString()), 4));
                    }
                }
            }
        }

        protected bool _ignoreAnnotations;
        protected List<object> _path = new List<object>();
        protected Type _type;
        #endregion
    }

    public class Conditional<T> : FieldReflector<T>
        where T : class, new()
    {
        public Conditional(string name, IForm<T> form, ConditionalDelegate<T> condition, bool ignoreAnnotations = false)
            : base(name, form, ignoreAnnotations)
        {
            _condition = condition;
        }

        public override bool Active(T state)
        {
            return _condition(state);
        }

        protected ConditionalDelegate<T> _condition;
    }

    public class Fields<T> : IFields<T>
    {
        public IField<T> Field(string name)
        {
            IField<T> field;
            _fields.TryGetValue(name, out field);
            return field;
        }

        public void Add(IField<T> field)
        {
            _fields[field.Name] = field;
        }

        public IEnumerator<IField<T>> GetEnumerator()
        {
            return (from entry in _fields select entry.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (from entry in _fields select entry.Value).GetEnumerator();
        }

        protected Dictionary<string, IField<T>> _fields = new Dictionary<string, IField<T>>();
    }
}
