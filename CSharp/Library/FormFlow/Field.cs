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

namespace Microsoft.Bot.Builder.FormFlow.Advanced
{
    #region Documentation
    /// <summary>   Define field delegate. </summary>
    /// <typeparam name="T">    Form state type. </typeparam>
    /// <param name="state">    Form state. </param>
    /// <param name="field">Field being dynamically defined.</param>
    /// <returns>True if field is defined.</returns>
    /// <remarks>Delegate for dynamically defining a field prompt and recognizer.  You can make use of the fluent methods
    ///          on <see cref="Field{T}"/> to change the characteristics of the field.</remarks>
    #endregion
    public delegate Task<bool> DefineAsyncDelegate<T>(T state, Field<T> field)
    where T : class;

    /// <summary>Base class with declarative implementation of <see cref="IField{T}"/>. </summary>
    /// <typeparam name="T">Underlying form state.</typeparam>
    public class Field<T> : IField<T>
        where T : class
    {
        /// <summary>   Construct field. </summary>
        /// <param name="name"> Name of field. </param>
        /// <param name="role"> Role field plays in form. </param>
        public Field(string name, FieldRole role)
        {
            _name = name;
            _role = role;
        }

        #region IField

        public string Name { get { return _name; } }

        public virtual IForm<T> Form
        {
            get { return this._form; }
            set
            {
                _form = value;
                foreach (var template in _form.Configuration.Templates)
                {
                    if (!_templates.ContainsKey(template.Usage))
                    {
                        AddTemplate(template);
                    }
                }
                if (_define == null)
                {
                    DefinePrompt();
                    DefineRecognizer();
                }
            }
        }

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

        public virtual Type Type
        {
            get { return _type; }
        }

        public virtual bool Optional
        {
            get
            {
                return _optional;
            }
        }

        public virtual bool IsNullable
        {
            get
            {
                return _isNullable;
            }
        }

        public virtual bool Limits(out double min, out double max)
        {
            min = _min;
            max = _max;
            return _limited;
        }

        public virtual IEnumerable<string> Dependencies
        {
            get
            {
                return _dependencies;
            }
        }
        #endregion

        #region IFieldDescription
        public virtual FieldRole Role
        {
            get
            {
                return _role;
            }
        }

        public virtual string FieldDescription
        {
            get
            {
                return _description;
            }
        }

        public virtual IEnumerable<string> FieldTerms
        {
            get
            {
                return _terms;
            }
        }

        public virtual IEnumerable<string> Terms(object value)
        {
            return _valueTerms[value];
        }

        public virtual string ValueDescription(object value)
        {
            return _valueDescriptions[value];
        }

        public virtual IEnumerable<string> ValueDescriptions
        {
            get
            {
                return (from entry in _valueDescriptions select entry.Value);
            }
        }

        public virtual IEnumerable<object> Values
        {
            get
            {
                return (from entry in _valueDescriptions select entry.Key);
            }
        }

        public virtual bool AllowsMultiple
        {
            get
            {
                return _allowsMultiple;
            }
        }

        public virtual bool AllowDefault
        {
            get
            {
                return _promptDefinition.AllowDefault != BoolDefault.False;
            }
        }

        public bool AllowNumbers
        {
            get
            {
                return _promptDefinition.AllowNumbers;
            }
        }
        #endregion

#if LOCALIZE
        #region IFieldResources

        public virtual void SaveResources()
        {
            var localizer = _form.Resources;
            localizer.Add(_name + nameof(_description), _description);
            localizer.Add(_name + nameof(_terms), _terms);
            localizer.Add(_name + nameof(_valueDescriptions), _valueDescriptions);
            localizer.Add(_name + nameof(_valueTerms), _valueTerms);
            if (_promptSet)
            {
                localizer.Add(_name + nameof(_promptDefinition), _promptDefinition.Patterns);
            }
            localizer.Add(_name, _templates);
        }

        public virtual void Localize()
        {
            var localizer = _form.Resources;
            localizer.Lookup(_name + nameof(_description), out _description);
            localizer.LookupValues(_name + nameof(_terms), out _terms);
            localizer.LookupDictionary(_name + nameof(_valueDescriptions), _valueDescriptions);
            localizer.LookupDictionary(_name + nameof(_valueTerms), _valueTerms);
            if (_promptSet)
            {
                string[] patterns;
                if (localizer.LookupValues(_name + nameof(_promptDefinition), out patterns))
                {
                    _promptDefinition.Patterns = patterns;
                }
            }
            localizer.LookupTemplates(_name, _templates);
            if (!_promptSet)
            {
                _promptDefinition = null;
            }
            _prompt = null;
            _recognizer = null;
            DefinePrompt();
            DefineRecognizer();
        }

        #endregion
#endif

        #region IFieldPrompt

        public virtual bool Active(T state)
        {
            return _condition(state);
        }

        public virtual TemplateAttribute Template(TemplateUsage usage)
        {
            TemplateAttribute template;
            _templates.TryGetValue(usage, out template);
            if (template != null)
            {
                template.ApplyDefaults(_form.Configuration.DefaultPrompt);
            }
            return template;
        }

        public virtual IPrompt<T> Prompt
        {
            get
            {
                return _prompt;
            }
        }

        public async virtual Task<bool> DefineAsync(T state)
        {
            bool result = true;
            if (_define != null)
            {
                if (!_promptSet)
                {
                    _promptDefinition = null;
                }
                _recognizer = null;
                _help = null;
                _prompt = null;
                result = await _define(state, this);
                DefinePrompt();
                DefineRecognizer();
            }
            return result;
        }

        public async virtual Task<ValidateResult> ValidateAsync(T state, object value)
        {
            return await _validate(state, value);
        }

        public virtual IPrompt<T> Help
        {
            get
            {
                return _help;
            }
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
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetFieldDescription(string description)
        {
            _description = description;
            return this;
        }

        /// <summary>   Set the terms associated with the field. </summary>
        /// <param name="terms">    The terms. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetFieldTerms(params string[] terms)
        {
            _terms = terms;
            return this;
        }

        /// <summary>   Adds a description for a value. </summary>
        /// <param name="value">        The value. </param>
        /// <param name="description">  Description of the value. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> AddDescription(object value, string description)
        {
            _valueDescriptions[value] = description;
            return this;
        }

        /// <summary>   Adds terms for a value. </summary>
        /// <param name="value">    The value. </param>
        /// <param name="terms">    The terms. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> AddTerms(object value, params string[] terms)
        {
            _valueTerms[value] = terms;
            return this;
        }

        /// <summary>   Removes the description and terms associated with a value. </summary>
        /// <param name="value">    The value to remove. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> RemoveValue(object value)
        {
            _valueDescriptions.Remove(value);
            _valueTerms.Remove(value);
            return this;
        }

        /// <summary>   Removes all values and their associated descriptions and terms. </summary>
        /// <returns>   A Field&lt;T&gt; </returns>
        public Field<T> RemoveValues()
        {
            _valueDescriptions.Clear();
            _valueTerms.Clear();
            return this;
        }

        /// <summary>   Sets the type of the underlying field state. </summary>
        /// <param name="type"> The field type. </param>
        /// <returns>   A Field&lt;T&gt; </returns>
        public Field<T> SetType(Type type)
        {
            _type = type;
            return this;
        }

        /// <summary>   Set whether or not a field is optional. </summary>
        /// <param name="optional"> True if field is optional. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetOptional(bool optional = true)
        {
            _optional = optional;
            return this;
        }

        #region Documentation
        /// <summary>   Sets whether or not multiple values are allowed. </summary>
        /// <param name="multiple"> True if multiple values are allowed. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        #endregion
        public Field<T> SetAllowsMultiple(bool multiple = true)
        {
            _allowsMultiple = multiple;
            return this;
        }

        /// <summary>   Set whether or not field is nullable. </summary>
        /// <param name="nullable"> True if field is nullable. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetIsNullable(bool nullable = true)
        {
            _isNullable = nullable;
            return this;
        }

        #region Documentation
        /// <summary>   Define a delegate for checking state to see if field applies. </summary>
        /// <param name="condition">    The condition delegate. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        #endregion
        public Field<T> SetActive(ActiveDelegate<T> condition)
        {
            _condition = condition;
            return this;
        }

        #region Documentation
        /// <summary>   Define a delegate for dynamically defining field. </summary>
        /// <param name="definition">   The definition delegate. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        /// <remarks>When you dynamically define a field through this delegate you can use all of the fluent methods
        ///          defined on <see cref="Field{T}"/> to change the descriptions and terms dynamically.</remarks>
        #endregion
        public Field<T> SetDefine(DefineAsyncDelegate<T> definition)
        {
            _define = definition;
            return this;
        }

        /// <summary>   Sets the field prompt. </summary>
        /// <param name="prompt">   The prompt. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetPrompt(PromptAttribute prompt)
        {
            _promptDefinition = prompt;
            _promptSet = true;
            return this;
        }

        /// <summary> Sets the recognizer for the field. </summary>
        /// <param name="recognizer">   The recognizer for the field. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetRecognizer(IRecognize<T> recognizer)
        {
            _recognizer = recognizer;
            return this;
        }

        /// <summary>   Replace a template in the field. </summary>
        /// <param name="template"> The template. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> ReplaceTemplate(TemplateAttribute template)
        {
            AddTemplate(template);
            return this;
        }

        /// <summary>   Set the field validation. </summary>
        /// <param name="validate"> The validator. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetValidate(ValidateAsyncDelegate<T> validate)
        {
            _validate = validate;
            return this;
        }

        /// <summary>   Set numeric limits. </summary>
        /// <param name="min">  The minimum. </param>
        /// <param name="max">  The maximum. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        public Field<T> SetLimits(double min, double max)
        {
            SetLimits(min, max, true);
            return this;
        }
        #region Documentation
        /// <summary>   Define the fields this field depends on. </summary>
        /// <param name="dependencies"> A variable-length parameters list containing dependencies. </param>
        /// <returns>   A <see cref="Field{T}"/>. </returns>
        #endregion
        public Field<T> SetDependencies(params string[] dependencies)
        {
            _dependencies = dependencies;
            return this;
        }
        #endregion

        #region Internals
        protected void DefinePrompt()
        {
            if (_promptDefinition == null)
            {
                TemplateUsage usage = TemplateUsage.None;
                if (_type == null || _type.IsEnum)
                {
                    usage = _allowsMultiple ? TemplateUsage.EnumSelectMany : TemplateUsage.EnumSelectOne;
                }
                else if (_type == typeof(string))
                {
                    usage = TemplateUsage.String;
                }
                else if (_type.IsIntegral())
                {
                    usage = TemplateUsage.Integer;
                }
                else if (_type == typeof(bool))
                {
                    usage = TemplateUsage.Bool;
                }
                else if (_type.IsDouble())
                {
                    usage = TemplateUsage.Double;
                }
                else if (_type == typeof(DateTime))
                {
                    usage = TemplateUsage.DateTime;
                }
                else
                {
                    throw new ArgumentException(_name + " is not a type FormFlow understands.");
                }
                if (usage != TemplateUsage.None)
                {
                    _promptDefinition = new PromptAttribute(Template(usage));
                }
                _promptSet = false;
            }
            _promptDefinition.ApplyDefaults(_form.Configuration.DefaultPrompt);
        }

        protected void DefineRecognizer()
        {
            if (_recognizer == null)
            {
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
                    _recognizer = new RecognizeNumber<T>(this, _form.Resources.Culture);
                }
                else if (_type.IsDouble())
                {
                    _recognizer = new RecognizeDouble<T>(this, _form.Resources.Culture);
                }
                else if (_type == typeof(DateTime))
                {
                    _recognizer = new RecognizeDateTime<T>(this, _form.Resources.Culture);
                }
                else if (_type.IsIEnumerable())
                {
                    var elt = _type.GetGenericElementType();
                    if (elt.IsEnum)
                    {
                        _recognizer = new RecognizeEnumeration<T>(this);
                    }
                }
                var template = Template(TemplateUsage.Help);
                _help = new Prompter<T>(template, _form, _recognizer);
                var prompt = _promptDefinition;
                _prompt = new Prompter<T>(_promptDefinition, _form, _recognizer);
            }
        }

        protected void SetLimits(double min, double max, bool limited)
        {
            _min = min;
            _max = max;
            _limited = limited;
        }

        protected void AddTemplate(TemplateAttribute template)
        {
            _templates[template.Usage] = template;
        }

        protected IForm<T> _form;
        protected string _name;
        protected FieldRole _role;
        protected ActiveDelegate<T> _condition = new ActiveDelegate<T>((state) => true);
        protected DefineAsyncDelegate<T> _define = null;
        protected ValidateAsyncDelegate<T> _validate = new ValidateAsyncDelegate<T>(async (state, value) => new ValidateResult { IsValid = true });
        protected double _min, _max;
        protected bool _limited;
        protected string[] _dependencies = new string[0];
        protected bool _allowsMultiple;
        protected Type _type;
        protected bool _optional;
        protected bool _isNullable;
        protected bool _keepZero;
        protected string _description;
        protected string[] _terms = new string[0];
        protected Dictionary<object, string> _valueDescriptions = new Dictionary<object, string>();
        protected Dictionary<object, string[]> _valueTerms = new Dictionary<object, string[]>();
        protected Dictionary<TemplateUsage, TemplateAttribute> _templates = new Dictionary<TemplateUsage, TemplateAttribute>();
        protected bool _promptSet;
        protected PromptAttribute _promptDefinition;
        protected IRecognize<T> _recognizer;
        protected IPrompt<T> _help;
        protected IPrompt<T> _prompt;
        #endregion
    }

    #region Documentation
    /// <summary>   Fill in field information through reflection.</summary>
    /// <remarks>   The resulting information can be overriden through the fluent interface.</remarks>
    /// <typeparam name="T">    The form state. </typeparam>
    #endregion
    public class FieldReflector<T> : Field<T>
        where T : class
    {
        #region Documentation
        /// <summary>   Construct an <see cref="IField{T}"/> through reflection. </summary>
        /// <param name="name">                 Path to the field in your form state. </param>
        /// <param name="ignoreAnnotations">    True to ignore annotations. </param>
        #endregion
        public FieldReflector(string name, bool ignoreAnnotations = false)
            : base(name, FieldRole.Value)
        {
            _ignoreAnnotations = ignoreAnnotations;
            AddField(typeof(T), _name.Split('.'), 0);
        }

        #region IField

        #region IFieldState
        public override object GetValue(T state)
        {
            object current = state;
            Type ftype = null;
            foreach (var step in _path)
            {
                ftype = StepType(step);
                var field = step as FieldInfo;
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
            }
            // Convert value types to null if appropriate
            return (ftype.IsEnum
                ? ((int)current == 0 ? null : current)
                : (ftype == typeof(DateTime) && ((DateTime)current) == DateTime.MinValue)
                    ? null
                    : current);
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
                foreach (var attribute in type.GetCustomAttributes(typeof(TemplateAttribute)))
                {
                    AddTemplate(attribute as TemplateAttribute);
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
                var describe = (field == null ? prop.GetCustomAttribute<DescribeAttribute>() : field.GetCustomAttribute<DescribeAttribute>());
                var terms = (field == null ? prop.GetCustomAttribute<TermsAttribute>() : field.GetCustomAttribute<TermsAttribute>());
                var prompt = (field == null ? prop.GetCustomAttribute<PromptAttribute>() : field.GetCustomAttribute<PromptAttribute>());
                var optional = (field == null ? prop.GetCustomAttribute<OptionalAttribute>() : field.GetCustomAttribute<OptionalAttribute>());
                var numeric = (field == null ? prop.GetCustomAttribute<NumericAttribute>() : field.GetCustomAttribute<NumericAttribute>());
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
                foreach (var attribute in (field == null ? prop.GetCustomAttributes<TemplateAttribute>() : field.GetCustomAttributes<TemplateAttribute>()))
                {
                    AddTemplate(attribute as TemplateAttribute);
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
                    var describe = enumField.GetCustomAttribute<DescribeAttribute>();
                    var terms = enumField.GetCustomAttribute<TermsAttribute>();
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

        /// <summary>   True to ignore annotations. </summary>
        protected bool _ignoreAnnotations;

        /// <summary>   Path to field value in state. </summary>
        protected List<object> _path = new List<object>();
        #endregion
    }

    public class Conditional<T> : FieldReflector<T>
        where T : class
    {
        public Conditional(string name, ActiveDelegate<T> condition, bool ignoreAnnotations = false)
            : base(name, ignoreAnnotations)
        {
            SetActive(condition);
        }
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

        /// <summary>   Mapping from field name to field definition. </summary>
        protected Dictionary<string, IField<T>> _fields = new Dictionary<string, IField<T>>();
    }
}
