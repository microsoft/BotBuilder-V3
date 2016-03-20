using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Form.Advanced
{
    /// <summary>
    /// Simple value table with explicitly added values.
    /// </summary>
    public class EnumeratedRecognizer<T> : IRecognizer<T>
        where T : class, new()
    {
        public delegate string DescriptionDelegate(object value);
        public delegate IEnumerable<string> TermsDelegate(object value);

        public EnumeratedRecognizer(IField<T> field)
        {
            var configuration = field.Form().Configuration();
            _form = field.Form();
            _description = field.Description();
            _terms = field.Terms();
            _values = field.Values();
            _valueDescriptions = field.ValueDescriptions();
            _descriptionDelegate = (value) => field.ValueDescription(value);
            _termsDelegate = (value) => field.Terms(value);
            _helpFormat = field.Template(field.AllowNumbers()
                ? (field.AllowsMultiple() ? TemplateUsage.EnumManyNumberHelp : TemplateUsage.EnumOneNumberHelp)
                : (field.AllowsMultiple() ? TemplateUsage.EnumManyWordHelp : TemplateUsage.EnumOneWordHelp));
            _noPreference = field.Optional() ? configuration.NoPreference : null;
            _currentChoice = configuration.CurrentChoice.FirstOrDefault();
            BuildPerValueMatcher(field.AllowNumbers(), configuration.NoPreference, configuration.CurrentChoice);
        }

        public EnumeratedRecognizer(IForm<T> form,
            string description,
            IEnumerable<object> terms,
            IEnumerable<object> values,
            DescriptionDelegate descriptionDelegate,
            TermsDelegate termsDelegate,
            bool allowNumbers,
            Template helpFormat,
            IEnumerable<string> noPreference = null,
            IEnumerable<string> currentChoice = null)
        {
            _values = values;
            _descriptionDelegate = descriptionDelegate;
            _termsDelegate = termsDelegate;
            _valueDescriptions = (from value in values select _descriptionDelegate(value)).ToArray();
            _helpFormat = helpFormat;
            _noPreference = noPreference;
            if (currentChoice != null)
            {
                _currentChoice = currentChoice.FirstOrDefault();
            }
            BuildPerValueMatcher(allowNumbers, noPreference, currentChoice);
        }

        public IEnumerable<object> Values()
        {
            return _values;
        }

        public IEnumerable<string> ValueDescriptions()
        {
            return _valueDescriptions;
        }

        public string ValueDescription(object value)
        {
            return _descriptionDelegate(value);
        }

        public IEnumerable<string> ValidInputs(object value)
        {
            return _termsDelegate(value);
        }

        public string Help(T state, object defaultValue)
        {
            var values = _valueDescriptions;
            if (_noPreference != null)
            {
                values = values.Union(new string[] { _noPreference.First() });
            }
            if ((defaultValue != null || _noPreference != null) && _currentChoice != null)
            {
                values = values.Union(new string[] { _currentChoice + " or 'c'" });
            }
            return new Prompter<T>(_helpFormat, _form, this).Prompt(state, "", 1, _max,
                Language.BuildList(values, _helpFormat.Separator, _helpFormat.LastSeparator));
        }

        public IEnumerable<TermMatch> Matches(string input, object defaultValue)
        {
            // if the user hit enter on an optional prompt, then consider taking the current choice as a low confidence option
            bool userSkippedPrompt = string.IsNullOrWhiteSpace(input) && (defaultValue != null || _noPreference != null);
            if (userSkippedPrompt)
            {
                yield return new TermMatch(0, input.Length, 1.0, defaultValue);
            }

            foreach (var expression in _expressions)
            {
                double longest = expression.Longest.Length;
                foreach (Match match in expression.Expression.Matches(input))
                {
                    var group1 = match.Groups[1];
                    var group2 = match.Groups[2];
                    if (group1.Success)
                    {
                        var confidence = System.Math.Min(group1.Length / longest, 1.0);
                        if (expression.Value is Special)
                        {
                            var special = (Special)expression.Value;
                            if (special == Special.CurrentChoice && (_noPreference != null || defaultValue != null))
                            {
                                yield return new TermMatch(group1.Index, group1.Length, confidence, defaultValue);
                            }
                            else if (special == Special.NoPreference)
                            {
                                yield return new TermMatch(group1.Index, group1.Length, confidence, null);
                            }
                        }
                        else
                        {
                            yield return new TermMatch(group1.Index, group1.Length, confidence, expression.Value);
                        }
                    }
                    else if (group2.Success)
                    {
                        yield return new TermMatch(group2.Index, group2.Length, 1.0, expression.Value);
                    }
                }
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("EnumeratedRecognizer({0}", _description);
            builder.Append(" [");
            foreach (var description in _valueDescriptions)
            {
                builder.Append(" ");
                builder.Append(description);
            }
            builder.Append("])");
            return builder.ToString();
        }

        protected enum Special { CurrentChoice, NoPreference };

        // Word character, any word character, any digit, any positive group over word characters
        protected const string WORD = @"(\w|\\w|\\d|(\[(?>(\w|-)+|\[(?<number>)|\](?<-number>))*(?(number)(?!))\]))";
        protected static Regex _wordStart = new Regex(string.Format(@"^{0}|\(", WORD), RegexOptions.Compiled);
        protected static Regex _wordEnd = new Regex(string.Format(@"({0}|\))(\?|\*|\+|\{{\d+\}}|\{{,\d+\}}|\{{\d+,\d+\}})?$", WORD), RegexOptions.Compiled);

        protected void BuildPerValueMatcher(bool allowNumbers, IEnumerable<string> noPreference, IEnumerable<string> currentChoice)
        {
            if (currentChoice != null)
            {
                // 0 is reserved for current default if any
                AddExpression(0, Special.CurrentChoice, currentChoice, allowNumbers);
            }
            var n = 1;
            foreach (var value in _values)
            {
                n = AddExpression(n, value, _termsDelegate(value), allowNumbers);
            }
            if (noPreference != null)
            {
                // Add recognizer for no preference
                n = AddExpression(n, Special.NoPreference, noPreference, allowNumbers);
            }
            if (_terms != null && _terms.Count() > 0)
            {
                // Add field terms to help disambiguate
                AddExpression(n, SpecialValues.Field, _terms, false);
            }
            _max = n - 1;
        }

        protected int AddExpression(int n, object value, IEnumerable<string> terms, bool allowNumbers)
        {
            var orderedTerms = (from term in terms orderby term.Length descending select term).ToArray();
            var word = new StringBuilder();
            var nonWord = new StringBuilder();
            var first = true;
            var firstNonWord = true;
            foreach (var term in orderedTerms)
            {
                var nterm = term.Trim().Replace(" ", @"\s+");
                if (nterm == "") nterm = "qqqq";
                if (_wordStart.Match(nterm).Success && _wordEnd.Match(nterm).Success)
                {
                    if (first)
                    {
                        first = false;
                        word.Append(@"(\b(?:");
                    }
                    else
                    {
                        word.Append('|');
                    }
                    word.Append(@"(?:");
                    word.Append(nterm);
                    word.Append(')');
                }
                else
                {
                    if (firstNonWord)
                    {
                        firstNonWord = false;
                        nonWord.Append('(');
                    }
                    else
                    {
                        nonWord.Append('|');
                    }
                    nonWord.Append(@"(?:");
                    nonWord.Append(nterm);
                    nonWord.Append(')');
                }
            }
            if (first)
            {
                word.Append("(qqqq)");
            }
            else
            {
                if (allowNumbers)
                {
                    if (n == 0)
                    {
                        word.Append("|c");
                    }
                    else
                    {
                        word.AppendFormat(@"|{0}", n);
                    }
                }
                word.Append(@")\b)");
            }
            if (firstNonWord)
            {
                nonWord.Append("(qqqq)");
            }
            else
            {
                nonWord.Append(')');
            }
            ++n;
            var expr = string.Format("{0}|{1}",
                word.ToString(),
                nonWord.ToString());
            _expressions.Add(new ValueAndExpression(value, new Regex(expr, RegexOptions.IgnoreCase), orderedTerms.First()));
            return n;
        }

        protected class ValueAndExpression
        {
            public ValueAndExpression(object value, Regex expression, string longest)
            {
                Value = value;
                Expression = expression;
                Longest = longest;
            }

            public readonly object Value;
            public readonly Regex Expression;
            public readonly string Longest;
        }

        protected IForm<T> _form;
        protected string _description;
        protected IEnumerable<string> _noPreference;
        protected string _currentChoice;
        protected IEnumerable<string> _terms;
        protected IEnumerable<object> _values;
        protected IEnumerable<string> _valueDescriptions;
        protected DescriptionDelegate _descriptionDelegate;
        protected TermsDelegate _termsDelegate;
        protected Template _helpFormat;
        protected int _max;
        protected List<ValueAndExpression> _expressions = new List<ValueAndExpression>();
    }

    public abstract class PrimitiveRecognizer<T> : IRecognizer<T>
        where T : class, new()
    {
        public PrimitiveRecognizer(IField<T> field)
        {
            _field = field;
            _currentChoices = new HashSet<string>(from choice in field.Form().Configuration().CurrentChoice
                                                  select choice.Trim().ToLower());
            if (field.Optional())
            {
                _noPreference = new HashSet<string>(from choice in field.Form().Configuration().NoPreference
                                                    select choice.Trim().ToLower());
            }
        }

        public abstract TermMatch Parse(string input);

        public virtual IEnumerable<TermMatch> Matches(string input, object defaultValue = null)
        {
            var matchValue = input.Trim().ToLower();
            if (_noPreference != null && _noPreference.Contains(matchValue))
            {
                yield return new TermMatch(0, input.Length, 1.0, null);
            }
            else if ((defaultValue != null || _noPreference != null) && (matchValue == "" || matchValue == "c" || _currentChoices.Contains(matchValue)))
            {
                yield return new TermMatch(0, input.Length, 1.0, defaultValue);
            }
            else {
                var result = Parse(input);
                if (result != null)
                {
                    yield return result;
                }
            }
        }

        public abstract IEnumerable<string> ValidInputs(object value);

        public abstract string ValueDescription(object value);

        public virtual IEnumerable<string> ValueDescriptions()
        {
            return new string[0];
        }

        public virtual IEnumerable<object> Values()
        {
            return null;
        }

        public abstract string Help(T state, object defaultValue);

        protected List<object> HelpArgs(T state, object defaultValue)
        {
            var args = new List<object>();
            if (defaultValue != null || _field.Optional())
            {
                args.Add(_field.Form().Configuration().CurrentChoice.First() + " or 'c'");
                if (_field.Optional())
                {
                    args.Add(_field.Form().Configuration().NoPreference.First());
                }
                else
                {
                    args.Add(null);
                }
            }
            else
            {
                args.Add(null);
                args.Add(null);
            }
            return args;
        }

        protected IField<T> _field;
        protected HashSet<string> _currentChoices;
        protected HashSet<string> _noPreference;
    }

    public class BoolRecognizer<T> : PrimitiveRecognizer<T>
        where T : class, new()
    {
        public BoolRecognizer(IField<T> field)
            : base(field)
        {
            if (field.Optional())
            {
                throw new ArgumentException("A bool field cannot be optional use an optional enumeration instead.");
            }
            _yes = new HashSet<string>(from term in field.Form().Configuration().Yes
                                       select term.Trim().ToLower());
            _no = new HashSet<string>(from term in field.Form().Configuration().No
                                      select term.Trim().ToLower());
        }

        public override TermMatch Parse(string input)
        {
            TermMatch result = null;
            var matchValue = input.Trim().ToLower();
            if (_yes.Contains(matchValue))
            {
                result = new TermMatch(0, input.Length, 1.0, true);
            }
            else if (_no.Contains(matchValue))
            {
                result = new TermMatch(0, input.Length, 1.0, false);
            }
            return result;
        }

        public override string Help(T state, object defaultValue)
        {
            var prompt = new Prompter<T>(_field.Template(TemplateUsage.BoolHelp), _field.Form(), null);
            var args = HelpArgs(state, defaultValue);
            return prompt.Prompt(state, _field.Name(), args.ToArray());
        }

        public override IEnumerable<string> ValidInputs(object value)
        {
            return (bool)value
                ? _field.Form().Configuration().Yes
                : _field.Form().Configuration().No;
        }

        public override string ValueDescription(object value)
        {
            return ((bool)value
                ? _field.Form().Configuration().Yes
                : _field.Form().Configuration().No).First();
        }

        protected HashSet<string> _yes;
        protected HashSet<string> _no;
    }

    public class StringRecognizer<T> : PrimitiveRecognizer<T>
        where T : class, new()
    {
        public StringRecognizer(IField<T> field)
            : base(field)
        {
        }

        public override IEnumerable<string> ValidInputs(object value)
        {
            yield return value as string;
        }

        public override string ValueDescription(object value)
        {
            return value as string;
        }

        public override TermMatch Parse(string input)
        {
            TermMatch result = null;
            if (!string.IsNullOrWhiteSpace(input))
            {
                // Confidence is 0.0 so commands get a crack
                result = new TermMatch(0, input.Length, 0.0, input);
            }
            return result;
        }

        public override string Help(T state, object defaultValue)
        {
            var prompt = new Prompter<T>(_field.Template(TemplateUsage.StringHelp), _field.Form(), null);
            var args = HelpArgs(state, defaultValue);
            return prompt.Prompt(state, _field.Name(), args.ToArray());
        }
    }

    public delegate string TypeValue(object value, CultureInfo culture);
    public delegate IEnumerable<TermMatch> Matcher(string input);

    public class LongRecognizer<T> : PrimitiveRecognizer<T>
        where T : class, new()
    {
        public LongRecognizer(IField<T> field, CultureInfo culture)
            : base(field)
        {
            _culture = culture;
            double min, max;
            _showLimits = field.Limits(out min, out max);
            _min = (long)min;
            _max = (long)max;
        }

        public override string ValueDescription(object value)
        {
            return ((long)Convert.ChangeType(value, typeof(long))).ToString(_culture.NumberFormat);
        }

        public override IEnumerable<string> ValidInputs(object value)
        {
            yield return ((long)value).ToString(_culture.NumberFormat);
        }

        public override TermMatch Parse(string input)
        {
            TermMatch result = null;
            long number;
            if (long.TryParse(input, out number))
            {
                if (number >= _min && number <= _max)
                {
                    result = new TermMatch(0, input.Length, 1.0, number);
                }
            }
            return result;
        }

        public override string Help(T state, object defaultValue)
        {
            var prompt = new Prompter<T>(_field.Template(TemplateUsage.IntegerHelp), _field.Form(), null);
            var args = HelpArgs(state, defaultValue);
            if (_showLimits)
            {
                args.Add(_min);
                args.Add(_max);
            }
            return prompt.Prompt(state, _field.Name(), args.ToArray());
        }

        protected long _min;
        protected long _max;
        protected bool _showLimits;
        protected CultureInfo _culture;
    }

    public class DoubleRecognizer<T> : PrimitiveRecognizer<T>
        where T : class, new()
    {
        public DoubleRecognizer(IField<T> field, CultureInfo culture)
            : base(field)
        {
            _culture = culture;
            _showLimits = field.Limits(out _min, out _max);
        }

        public override string ValueDescription(object value)
        {
            return ((double)Convert.ChangeType(value, typeof(double))).ToString(_culture.NumberFormat);
        }

        public override IEnumerable<string> ValidInputs(object value)
        {
            yield return ((double)value).ToString(_culture.NumberFormat);
        }

        public override TermMatch Parse(string input)
        {
            TermMatch result = null;
            double number;
            if (double.TryParse(input, out number))
            {
                if (number >= _min && number <= _max)
                {
                    result = new TermMatch(0, input.Length, 1.0, number);
                }
            }
            return result;
        }

        public override string Help(T state, object defaultValue)
        {
            var prompt = new Prompter<T>(_field.Template(TemplateUsage.DoubleHelp), _field.Form(), null);
            var args = HelpArgs(state, defaultValue);
            if (_showLimits)
            {
                args.Add(_min);
                args.Add(_max);
            }
            return prompt.Prompt(state, _field.Name(), args.ToArray());
        }

        protected double _min;
        protected double _max;
        protected bool _showLimits;
        protected CultureInfo _culture;
    }

    /* TODO: Implement more recognizers.  May want to use built-in datetime parser.
    /// <summary>
    /// Regular expression recognizer.  For example if you had a DateTime field you would 
    /// have this format the date for the culture and use regexs to recognize date/times.
    /// </summary>
    public abstract class RegexRecognizer<T> : IRecognizer<T>
        where T : class, new()
    {
        public RegexRecognizer(IFieldDescription fieldDescription)
        {
            _fieldDescription = fieldDescription;
        }

        public abstract IEnumerable<string> ValueDescriptions();

        public abstract string ValueDescription(object value);

        public IEnumerable<object> Values()
        {
            return null;
        }

        public abstract IEnumerable<string> ValidInputs(object value);

        public abstract string Help(T state, object defaultValue);

        public abstract IEnumerable<TermMatch> Matches(string input, object defaultValue);

        protected IFieldDescription _fieldDescription;
    }

    public class DateRecognizer : RegexRecognizer
    {
        private static Regex _regex = new Regex(@"(?:^|\s)(?<Month>\d{1,2})/(?<Day>\d{1,2})/(?<Year>(?:\d{4}|\d{2}))(?:\s|$)", RegexOptions.Compiled);

        public DateRecognizer(IFieldDescription fieldDescription, string valueDescription)
            : base(fieldDescription)
        {
            _valueDescription = valueDescription;
        }

        public override IEnumerable<string> ValidInputs(object value)
        {
            yield return ((DateTime)value).ToString(_fieldDescription.Culture().DateTimeFormat);
        }

        public override IEnumerable<string> ValueDescriptions()
        {
            yield return _valueDescription;
        }

        public override string ValueDescription(object value)
        {
            return ((DateTime)value).ToString(_fieldDescription.Culture().DateTimeFormat);
        }

        public override IEnumerable<TermMatch> Matches(string input, object defaultValue, bool allowNull)
        {
            foreach (Match match in _regex.Matches(input))
            {
                if (match.Success)
                {
                    var group = match.Groups[0];
                    var month = int.Parse(match.Groups["Month"].Value);
                    var day = int.Parse(match.Groups["Day"].Value);
                    var year = int.Parse(match.Groups["Year"].Value);
                    if (year < 100) year += 2000;
                    var date = new DateTime();
                    bool ok = false;
                    try
                    {
                        date = new DateTime(year, month, day);
                        ok = true;
                    }
                    catch (Exception)
                    { }
                    if (ok)
                    {
                        yield return new TermMatch(group.Index, group.Length, 1.0, date);
                    }
                }
            }
        }

        private string _valueDescription;
    }
    */
}
