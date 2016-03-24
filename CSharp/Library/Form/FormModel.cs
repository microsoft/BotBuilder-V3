using System.Collections.Generic;

using Microsoft.Bot.Builder.Form.Advanced;

namespace Microsoft.Bot.Builder.Form
{
    internal sealed class FormModel<T> : IFormModel<T>
        where T : class, new()
    {
        internal readonly bool _ignoreAnnotations;
        internal readonly FormConfiguration _configuration;
        internal readonly Fields<T> _fields;
        internal readonly List<IStep<T>> _steps;
        internal CompletionDelegate<T> _completion;

        public FormModel(bool ignoreAnnotations, FormConfiguration configuration = null, Fields<T> fields = null, List<IStep<T>> steps = null, CompletionDelegate<T> completion = null)
        {
            this._ignoreAnnotations = ignoreAnnotations;
            this._configuration = configuration ?? new FormConfiguration();
            this._fields = fields ?? new Fields<T>();
            this._steps = steps ?? new List<IStep<T>>();
            this._completion = completion;
        }

        internal override bool IgnoreAnnotations
        {
            get
            {
                return this._ignoreAnnotations;
            }
        }

        internal override FormConfiguration Configuration
        {
            get
            {
                return _configuration;
            }
        }

        internal override IReadOnlyList<IStep<T>> Steps
        {
            get
            {
                return this._steps;
            }
        }

        internal override CompletionDelegate<T> Completion
        {
            get
            {
                return this._completion;
            }
        }

        internal override IFields<T> Fields
        {
            get
            {
                return this._fields;
            }
        }
    }
}
