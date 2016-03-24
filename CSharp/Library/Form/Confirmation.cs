using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Form.Advanced
{
    /// <summary>
    /// Confirmation 
    /// </summary>
    /// <typeparam name="T">Form state.</typeparam>
    public class Confirmation<T> : Field<T>
        where T : class, new()
    {
        /// <summary>
        /// Construct a confirmation.
        /// </summary>
        /// <param name="name">Name of this </param>
        /// <param name="prompt">Confirmation prompt expressed using \ref patterns.</param>
        /// <param name="condition">Delegate for whether confirmation applies.</param>
        /// <param name="dependencies">Fields that must have values before confirmation can run.</param>
        /// <param name="model">The form model.</param>
        public Confirmation(Prompt prompt, ConditionalDelegate<T> condition, IEnumerable<string> dependencies, IFormModel<T> model)
            : base(Guid.NewGuid().ToString(), FieldRole.Confirm, model)
        {
            this
                .Description(_name)
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

        public override void SetValue(T state, object value)
        {
            throw new NotImplementedException();
        }

        public override bool IsUnknown(T state)
        {
            return true;
        }

        public override void SetUnknown(T state)
        {
            throw new NotImplementedException();
        }

        public override IPrompt<T> Prompt()
        {
            if (_prompter == null)
            {
                _prompter = new Prompter<T>(_promptDefinition, _model, new RecognizeBool<T>(this));
            }
            return _prompter;
        }
        #endregion

        #region Implementation
        private delegate NextStep NextDelegate(bool response, T state);
        private readonly ConditionalDelegate<T> _condition;
        private readonly string[] _dependencies;
        private IPrompt<T> _prompter;
        private readonly NextDelegate _next;
        #endregion
    }
}
