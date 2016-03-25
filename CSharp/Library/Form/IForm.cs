﻿using System.Collections.Generic;
using System.Linq;

using Microsoft.Bot.Builder.Form.Advanced;

namespace Microsoft.Bot.Builder.Form
{
    public abstract class IForm<T>
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
            var field = new Field<T>("__commands__", FieldRole.Value, form);
            field.Prompt(new Prompt(""));
            field.Description("Commands");
            field.Terms(new string[0]);
            foreach (var entry in form.Configuration.Commands)
            {
                field.AddDescription(entry.Key, entry.Value.Description);
                field.AddTerms(entry.Key, entry.Value.Terms);
            }
            foreach (var nav in form.Fields)
            {
                var fterms = nav.Terms();
                if (fterms != null)
                {
                    field.AddDescription(nav.Name, nav.Description());
                    field.AddTerms(nav.Name, fterms.ToArray());
                }
            }
            var commands = new RecognizeEnumeration<T>(field);
            return commands;
        }
    }
}
