using System;
using System.Linq;

namespace Microsoft.Bot.Builder.Form
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Struct)]
    public class Describe : Attribute
    {
        public readonly string Description;

        public Describe(string description)
        {
            Description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Struct)]
    public class Terms : Attribute
    {
        public string[] Alternatives;

        private int _maxPhrase;
        public int MaxPhrase
        {
            get
            {
                return _maxPhrase;
            }
            set
            {
                _maxPhrase = value;
                Alternatives = Alternatives.SelectMany(alt => Advanced.Language.GenerateTerms(alt, _maxPhrase)).ToArray();
            }
        }

        public Terms(string[] alternatives)
        {
            Alternatives = alternatives;
        }

        public Terms(string root)
        {
            Alternatives = new string[] { root };
        }
    }

    public enum PromptStyle { Default, Auto, Inline, PerLine };
    public enum PromptNormalization { Default, Auto, Lower, Upper };
    public enum BoolDefault { Default, Yes, No };
    public enum FeedbackOptions { Default, Auto, Always, Never };

    public abstract class PromptBase : Attribute
    {
        private readonly string[] _templates;
        private static Random _generator = new Random();

        public BoolDefault AllowDefault { get; set; }

        public BoolDefault AllowNumbers { get; set; }

        public PromptNormalization Case { get; set; }

        public FeedbackOptions Feedback { get; set; }

        public string Format { get; set; }

        public string LastSeparator { get; set; }

        public string Separator { get; set; }

        public PromptStyle Style { get; set; }

        public string Template()
        {
            var choice = 0;
            if (_templates.Length > 1)
            {
                choice = _generator.Next(_templates.Length);
            }
            return _templates[choice];
        }

        public string[] Templates()
        {
            return _templates;
        }

        public PromptBase ApplyDefaults(PromptBase defaultPrompt)
        {
            if (AllowDefault == BoolDefault.Default) AllowDefault = defaultPrompt.AllowDefault;
            if (AllowNumbers == BoolDefault.Default) AllowNumbers = defaultPrompt.AllowNumbers;
            if (Case == PromptNormalization.Default) Case = defaultPrompt.Case;
            if (Feedback == FeedbackOptions.Default) Feedback = defaultPrompt.Feedback;
            if (Format == null) Format = defaultPrompt.Format;
            if (LastSeparator == null) LastSeparator = defaultPrompt.LastSeparator;
            if (Separator == null) Separator = defaultPrompt.Separator;
            if (Style == PromptStyle.Default) Style = defaultPrompt.Style;
            return this;
        }

        public PromptBase(string template)
        {
            _templates = new string[] { template };
        }

        public PromptBase(string[] templates)
        {
            _templates = templates;
            Initialize();
        }

        public PromptBase(PromptBase other)
        {
            _templates = other._templates;
            AllowDefault = other.AllowDefault;
            AllowNumbers = other.AllowNumbers;
            Case = other.Case;
            Feedback = other.Feedback;
            Format = other.Format;
            LastSeparator = other.LastSeparator;
            Separator = other.Separator;
            Style = other.Style;
        }

        private void Initialize()
        {
            Style = PromptStyle.Default;
            Format = null;
            Separator = null;
            LastSeparator = null;
            Case = PromptNormalization.Default;
            AllowDefault = BoolDefault.Default;
            AllowNumbers = BoolDefault.Default;
        }
    }

    // Prompt is on a field/property only, is named and uses the fields recognizer
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class Prompt : PromptBase
    {
        public Prompt(string template)
            : base(template)
        {
        }

        public Prompt(string[] templates)
            : base(templates)
        { }

        public Prompt(Template template)
            : base(template)
        {
        }
    }

    public enum TemplateUsage
    {
        Clarify, CurrentChoice, DateTime, Double, Feedback,
        Help, HelpClarify, HelpDateTime, HelpDouble, HelpInteger, HelpNavigation, HelpOneNumber, HelpManyNumber, HelpOneWord, HelpManyWord, HelpString,
        Integer, NextStep, NoPreference, NotUnderstood, SelectOne, SelectMany, String, Unspecified
    };

    // This is a template for creating prompts
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class Template : PromptBase
    {

        public readonly TemplateUsage Usage;

        public Template(TemplateUsage usage, string template)
            : base(template)
        {
            Usage = usage;
        }

        public Template(TemplateUsage usage, string[] templates)
            : base(templates)
        {
            Usage = usage;
        }

        private void Initialize()
        {
            AllowDefault = (Usage != TemplateUsage.Clarify) ? BoolDefault.Yes : BoolDefault.No;
            Case = PromptNormalization.Default;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class Optional : Attribute
    {
        public Optional()
        { }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class Numeric : Attribute
    {
        public readonly double Min;
        public readonly double Max;

        public Numeric(double min, double max)
        {
            Min = min;
            Max = max;
        }
    }
}
