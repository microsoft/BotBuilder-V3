using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Form.Advanced
{
    public delegate Template TemplateDelegate(TemplateUsage usage);

    public class Confirmation<T> : FieldReflector<T>
        where T : class, new()
    {
        public Confirmation(string name, Prompt prompt, ConditionalDelegate<T> condition, IEnumerable<string> dependencies)
            : base("")
        {
            _name = name;
            _valueDescriptions.Clear();
            _valueTerms.Clear();
            this
                .Description(name)
                .Prompt(prompt)
                .AddDescription(true, "Yes")
                .AddTerms(true, new string[] { "yes", "y", "sure", "ok" })
                .AddDescription(false, "No")
                .AddTerms(false, new string[] { "no", "n" })
                ;
            _condition = condition;
            _dependencies = dependencies.ToArray();
            var noStep = (dependencies.Count() > 0 ? new NextStep(dependencies) : new NextStep());
            _next = (value, state) => value ? new NextStep() : noStep;
        }

        public override object GetValue(T state)
        {
            return null;
        }

        public override IEnumerable<string> Dependencies()
        {
            return _dependencies;
        }

        #region IFieldPrompt
        public override bool Active(T state)
        {
            return _condition(state);
        }

        public override NextStep Next(object value, T state)
        {
            return _next((bool) value, state);
        }

        public override FieldRole Role()
        {
            return FieldRole.Confirm;
        }
        #endregion

        #region Implementation
        private readonly ConditionalDelegate<T> _condition;
        private readonly string[] _dependencies;
        private readonly NextDelegate<T> _next;
        #endregion
    }
}
