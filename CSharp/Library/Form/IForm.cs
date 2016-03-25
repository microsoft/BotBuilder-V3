using System.Collections.Generic;
using System.Linq;

using Microsoft.Bot.Builder.Form.Advanced;

namespace Microsoft.Bot.Builder.Form
{
    public abstract class IForm<T>
        where T : class, new()
    {
        internal abstract bool IgnoreAnnotations { get; }
        internal abstract FormConfiguration Configuration { get; }
        internal abstract IReadOnlyList<IStep<T>> Steps { get; }
        internal abstract CompletionDelegate<T> Completion { get; }
        internal abstract IFields<T> Fields { get; }
    }   

    public static partial class Extension
    {
        internal static IStep<T> Step<T>(this IForm<T> form, string name) where T : class, new()
        {
            IStep<T> result = null;
            foreach (var step in form.Steps)
            {
                if (step.Name == name)
                {
                    result = step;
                    break;
                }
            }
            return result;
        }

        internal static int StepIndex<T>(this IForm<T> form, IStep<T> step) where T : class, new()
        {
            var index = -1;
            for (var i = 0; i < form.Steps.Count; ++i)
            {
                if (form.Steps[i] == step)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        internal static IRecognize<T> BuildCommandRecognizer<T>(this IForm<T> form) where T : class, new()
        {
            var values = new List<object>();
            var descriptions = new Dictionary<object, string>();
            var terms = new Dictionary<object, string[]>();
            foreach (var entry in form.Configuration.Commands)
            {
                values.Add(entry.Key);
                descriptions[entry.Key] = entry.Value.Description;
                terms[entry.Key] = entry.Value.Terms;
            }
            foreach (var field in form.Fields)
            {
                var fterms = field.Terms();
                if (fterms != null)
                {
                    values.Add(field.Name);
                    descriptions.Add(field.Name, field.Description());
                    terms.Add(field.Name, fterms.ToArray());
                }
            }
            var commands = new RecognizeEnumeration<T>(form, "Form commands", null,
                values,
                    (value) => descriptions[value],
                    (value) => terms[value],
                    false, null);

            return commands;
        }
    }
}
