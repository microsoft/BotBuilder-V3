using Microsoft.Bot.Builder.Form.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Form
{
    internal enum StepPhase { Ready, Responding, Completed };
    internal enum StepType { Field, Confirm, Navigation, Message };
    internal interface IStep<T>
        where T : class, new()

    {
        string Name { get; }

        StepType Type { get; }

        IField<T> Field { get; }

        bool Active(T state);

        string Start(IDialogContext context, T state, FormState form);

        IEnumerable<TermMatch> Match(IDialogContext context, T state, FormState form, string input, out string lastInput);

        NextStep Process(IDialogContext context, T state, FormState form, string input, IEnumerable<TermMatch> matches,
            out string feedback, out string prompt);

        string NotUnderstood(IDialogContext context, T state, FormState form, string input);

        string Help(T state, FormState form, string commandHelp);

        bool Back(IDialogContext context, T state, FormState form);

        IEnumerable<string> Dependencies();
    }

}
