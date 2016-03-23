using Microsoft.Bot.Builder.Form.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Form
{
    public abstract class IFormModel<T> : Builder.Fibers.Serialization.ISerializeAsReference
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
        internal static IStep<T> Step<T>(this IFormModel<T> spec, string name) where T : class, new()
        {
            IStep<T> result = null;
            foreach (var step in spec.Steps)
            {
                if (step.Name == name)
                {
                    result = step;
                    break;
                }
            }
            return result;
        }

        internal static int StepIndex<T>(this IFormModel<T> spec, IStep<T> step) where T : class, new()
        {
            var index = -1;
            for (var i = 0; i < spec.Steps.Count; ++i)
            {
                if (spec.Steps[i] == step)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

    }
}
