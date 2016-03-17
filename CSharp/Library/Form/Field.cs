using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.Bot.Builder.Form.Advanced
{
    public abstract class Field<T> : IField<T>
        where T : class, new()
    {
        public Field(string name, FieldRole role)
        {
            _name = name;
            _role = role;
        }

        #region IField

        public string Name()
        {
            return _name;
        }

        public virtual IForm<T> Form()
        {
            return _form;
        }

        public virtual void SetForm(IForm<T> form)
        {
            _form = form;
            foreach (var template in form.Configuration().Templates)
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

        #region IFieldState
        public abstract object GetValue(T state);

        public abstract void SetValue(T state, object value);

        public abstract bool IsUnknown(T state);

        public abstract void SetUnknown(T state);

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
        public virtual bool AllowsMultiple()
        {
            return _allowsMultiple;
        }

        public virtual FieldRole Role()
        {
            return _role;
        }

        public virtual string Description()
        {
            return _description;
        }

        public virtual bool Optional()
        {
            return _optional;
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
            return template;
        }

        public abstract IPrompt<T> Prompt();

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
        public Field<T> Description(string description)
        {
            UpdateAnnotations();
            _description = description;
            return this;
        }

        public Field<T> Help(string help)
        {
            UpdateAnnotations();
            _help = new Prompt(help);
            return this;
        }

        public Field<T> Help(Prompt help)
        {
            UpdateAnnotations();
            _help = help;
            return this;
        }

        public Field<T> Terms(IEnumerable<string> terms)
        {
            UpdateAnnotations();
            _terms = terms.ToArray();
            return this;
        }

        public Field<T> AddDescription(object value, string description)
        {
            UpdateAnnotations();
            _valueDescriptions[value] = description;
            return this;
        }

        public Field<T> AddTerms(object value, IEnumerable<string> terms)
        {
            UpdateAnnotations();
            _valueTerms[value] = terms.ToArray();
            return this;
        }

        public Field<T> Optional(bool optional = true)
        {
            UpdateAnnotations();
            _optional = optional;
            return this;
        }

        public bool AllowDefault()
        {
            return _promptDefinition.AllowDefault != BoolDefault.No;
        }

        public void SetLimits(double min, double max, bool limited)
        {
            _min = min;
            _max = max;
            _limited = limited;
        }

        /// <summary>
        /// Allow numbers to be matched.
        /// </summary>
        /// <returns>True if numbers are allowed as input.</returns>
        public bool AllowNumbers()
        {
            return _promptDefinition.AllowNumbers != BoolDefault.No;
        }

        public Field<T> Prompt(Prompt prompt)
        {
            UpdateAnnotations();
            _promptDefinition = prompt;
            return this;
        }

        public Field<T> Template(Template template)
        {
            UpdateAnnotations();
            AddTemplate(template);
            return this;
        }
        #endregion

        #region Internals
        protected void UpdateAnnotations()
        {
            if (_form != null)
            {
                throw new ArgumentException("Cannot modify field annotations once added to a form.");
            }
        }

        protected void AddTemplate(Template template)
        {
            _templates[template.Usage] = template;
        }

        protected string _name;
        protected IForm<T> _form;
        protected FieldRole _role;
        protected double _min, _max;
        protected bool _limited;
        protected bool _allowsMultiple;
        protected bool _optional;
        protected string _description;
        protected Prompt _help;
        protected string[] _terms;
        protected Dictionary<object, string> _valueDescriptions = new Dictionary<object, string>();
        protected Dictionary<object, string[]> _valueTerms = new Dictionary<object, string[]>();
        protected Dictionary<TemplateUsage, Template> _templates = new Dictionary<TemplateUsage, Template>();
        protected Prompt _promptDefinition;
        #endregion
    }

    public class FieldReflector<T> : Field<T>
        where T : class, new()
    {
        public FieldReflector(string name, bool ignoreAnnotations = false)
            : base(name, FieldRole.Value)
        {
            _ignoreAnnotations = ignoreAnnotations;
            AddField(typeof(T), string.IsNullOrWhiteSpace(_name) ? new string[] { } : _name.Split('.'), 0);
        }

        #region IField
        public override void SetForm(IForm<T> form)
        {
            base.SetForm(form);
            if (_promptDefinition == null)
            {
                if (_type.IsEnum)
                {
                    _promptDefinition = new Prompt(Template(_allowsMultiple ? TemplateUsage.SelectMany : TemplateUsage.SelectOne));
                }
                else if (_type == typeof(string))
                {
                    _promptDefinition = new Prompt(Template(TemplateUsage.String));
                }
                else if (_type.IsIntegral())
                {
                    _promptDefinition = new Prompt(Template(TemplateUsage.Integer));
                }
                /* TODO: 
                else if (_type.IsDouble())
                {
                    _promptDefinition = new Prompt(Template(TemplateUsage.Double));
                }
                else if (_type == typeof(DateTime))
                {
                    _promptDefinition = new Prompt(Template(TemplateUsage.DateTime));
                }
                */
            }
        }

        #region IFieldState
        public override object GetValue(T state)
        {
            object current = state;
            bool isEnum = false;
            foreach (var field in _path)
            {
                current = field.GetValue(current);
                if (current == null)
                {
                    break;
                }
                isEnum = field.FieldType.IsEnum;
            }
            return isEnum ? ((int)current == 0 ? null : current) : current;
        }


        public override void SetValue(T state, object value)
        {
            object current = state;
            object lastClass = state;
            FieldInfo last = _path.Last();
            foreach (var field in _path)
            {
                if (field == last)
                {
                    var ftype = field.FieldType;
                    if (ftype.IsIEnumerable())
                    {
                        if (value == null)
                        {
                            field.SetValue(lastClass, null);
                        }
                        else if (ftype == typeof(string))
                        {
                            field.SetValue(lastClass, value);
                        }
                        else
                        {
                            // Build list and coerce elements
                            var list = Activator.CreateInstance(field.FieldType);
                            var addMethod = list.GetType().GetMethod("Add");
                            foreach (var elt in value as System.Collections.IEnumerable)
                            {
                                addMethod.Invoke(list, new object[] { elt });
                            }
                            field.SetValue(lastClass, list);
                        }
                    }
                    else
                    {
                        if (value == null && (ftype.IsEnum || ftype.IsIntegral()))
                        {
                            field.SetValue(lastClass, 0);
                        }
                        else if (ftype.IsIntegral())
                        {
                            field.SetValue(lastClass, Convert.ChangeType(value, ftype));
                        }
                        else
                        {
                            field.SetValue(lastClass, value);
                        }
                    }
                }
                else
                {
                    current = field.GetValue(current);
                    if (current == null)
                    {
                        var obj = Activator.CreateInstance(field.FieldType);
                        field.SetValue(lastClass, obj);
                        current = obj;
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
                var type = _path.Last().FieldType;
                if (type.IsValueType && type.IsEnum)
                {
                    unknown = ((int)value == 0);
                }
                else if (type.IsIEnumerable())
                {
                    unknown = !(value as System.Collections.IEnumerable).GetEnumerator().MoveNext();
                }
            }
            return unknown;
        }

        public override void SetUnknown(T state)
        {
            FieldInfo last = _path.Last();
            if (last.FieldType.IsEnum)
            {
                SetValue(state, 0);
            }
            else
            {
                SetValue(state, null);
            }
        }

        #endregion

        #region IFieldPrompt

        public override IPrompt<T> Prompt()
        {
            if (_prompt == null)
            {
                var field = _path.LastOrDefault();
                var ftype = (field == null ? null : field.FieldType);
                IRecognizer<T> recognizer = null;
                if (ftype == null || ftype.IsEnum)
                {
                    recognizer = new EnumeratedRecognizer<T>(this);
                }
                else if (ftype == typeof(string))
                {
                    recognizer = new StringRecognizer<T>(this);
                }
                else if (ftype.IsIEnumerable())
                {
                    var elt = ftype.GetGenericElementType();
                    if (elt.IsEnum)
                    {
                        recognizer = new EnumeratedRecognizer<T>(this);
                    }
                }
                else if (ftype.IsIntegral())
                {
                    recognizer = new LongRecognizer<T>(this, CultureInfo.CurrentCulture);
                }
                _prompt = new Prompter<T>(_promptDefinition, _form, recognizer);
            }
            return _prompt;
        }

        #endregion
        #endregion

        #region Internals
        protected void AddField(Type type, string[] path, int ipath)
        {
            if (ipath < path.Length)
            {
                ProcessTemplates(type);
                var step = path[ipath];
                var field = type.GetField(step, BindingFlags.Public | BindingFlags.Instance);
                var ftype = field.FieldType;
                _path.Add(field);
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
                else if (ftype.IsEnum)
                {
                    _type = ftype;
                    ProcessFieldAttributes(field);
                    ProcessEnumAttributes(ftype);
                }
                else if (ftype.IsIntegral())
                {
                    _type = ftype;
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
                    // TODO: double recognizer
                }
                else if (ftype == typeof(DateTime))
                {
                    // Datetime recognizer
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

        protected void ProcessFieldAttributes(FieldInfo field)
        {
            _optional = false;
            if (!_ignoreAnnotations)
            {
                var describe = field.GetCustomAttribute<Describe>();
                var terms = field.GetCustomAttribute<Terms>();
                var prompt = field.GetCustomAttribute<Prompt>();
                var optional = field.GetCustomAttribute<Optional>();
                var numeric = field.GetCustomAttribute<Numeric>();
                if (describe != null)
                {
                    _description = describe.Description;
                }
                else
                {
                    _description = Language.CamelCase(field.Name);
                }
                if (terms != null)
                {
                    _terms = terms.Alternatives;
                }
                else
                {
                    _terms = Language.GenerateTerms(field.Name, 3);
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
                foreach (var attribute in field.GetCustomAttributes<Template>())
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
                if ((int)enumValue > 0)
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
                        _valueTerms.Add(enumValue, Language.GenerateTerms(enumValue.ToString(), 4));
                    }
                }
            }
        }

        protected bool _ignoreAnnotations;
        protected List<FieldInfo> _path = new List<FieldInfo>();
        protected Type _type;
        protected IPrompt<T> _prompt;
        #endregion
    }

    public class Conditional<T> : FieldReflector<T>
        where T : class, new()
    {
        public Conditional(string name, ConditionalDelegate<T> condition, bool ignoreAnnotations = false)
            : base(name, ignoreAnnotations)
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
        where T : class, new()
    {
        public IField<T> Field(string name)
        {
            IField<T> field;
            _fields.TryGetValue(name, out field);
            return field;
        }

        public void Add(IField<T> field)
        {
            _fields[field.Name()] = field;
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
