// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Internals.Scorables
{
    // TODO: more generic name, or reuse existing attribute for overriding name?
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    [Serializable]
    public sealed class EntityAttribute : Attribute
    {
        public readonly string Entity;
        public EntityAttribute(string entity)
        {
            SetField.NotNull(out this.Entity, nameof(entity), entity);
        }
    }

    /// <summary>
    /// Scorable to represent binding arguments to a method's parameters.
    /// </summary>
    public sealed class MethodScorable : ScorableBase<IResolver, Binding, Binding>
    {
        private readonly MethodInfo method;
        private readonly ParameterInfo[] parameters;
        public MethodScorable(MethodInfo method)
        {
            SetField.NotNull(out this.method, nameof(method), method);
            this.parameters = this.method.GetParameters();
        }
        public override string ToString()
        {
            return $"{this.GetType().Name}({method})";
        }
        private bool TryResolveInstance(IResolver resolver, out object instance)
        {
            if (this.method.IsStatic)
            {
                instance = null;
                return true;
            }

            return resolver.TryResolve(this.method.DeclaringType, null, out instance);
        }
        private static bool TryResolveArgument(IResolver resolver, ParameterInfo parameter, out object argument)
        {
            var entity = parameter.GetCustomAttribute<EntityAttribute>();
            if (entity != null)
            {
                return resolver.TryResolve(parameter.ParameterType, entity.Entity, out argument);
            }

            return resolver.TryResolve(parameter.ParameterType, parameter.Name, out argument);
        }
        private bool TryResolveArguments(IResolver resolver, out object[] arguments)
        {
            if (this.parameters.Length == 0)
            {
                arguments = Array.Empty<object>();
                return true;
            }

            arguments = null;
            for (int index = 0; index < this.parameters.Length; ++index)
            {
                var parameter = this.parameters[index];

                object argument;

                if (!TryResolveArgument(resolver, parameter, out argument))
                {
                    arguments = null;
                    return false;
                }

                if (arguments == null)
                {
                    arguments = new object[this.parameters.Length];
                }

                arguments[index] = argument;
            }

            return arguments != null;
        }

        public override Task<Binding> PrepareAsync(IResolver item, CancellationToken token)
        {
            try
            {
                object instance;
                if (!TryResolveInstance(item, out instance))
                {
                    return Tasks<Binding>.Null;
                }

                object[] arguments;
                if (!TryResolveArguments(item, out arguments))
                {
                    return Tasks<Binding>.Null;
                }

                var binding = new Binding(this.method, this.parameters, instance, arguments);
                return Task.FromResult(binding);
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled<Binding>(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException<Binding>(error);
            }
        }
        public override bool HasScore(IResolver resolver, Binding state)
        {
            return state != null;
        }
        public override Binding GetScore(IResolver resolver, Binding state)
        {
            return state;
        }
        public override Task PostAsync(IResolver item, Binding state, CancellationToken token)
        {
            try
            {
                return state.InvokeAsync(token);
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled<Binding>(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException(error);
            }
        }
    }
}
