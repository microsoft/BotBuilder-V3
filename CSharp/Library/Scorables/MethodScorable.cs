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
using Microsoft.Bot.Builder.Scorables.Internals;

namespace Microsoft.Bot.Builder.Scorables
{
    /// <summary>
    /// This attribute is used to specify that a method will participate in the
    /// scoring process for overload resolution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [Serializable]
    public sealed class MethodBindAttribute : Attribute
    {
    }

    /// <summary>
    /// This attribute is used to specify that a method parameter is bound to an entity
    /// that can be resolved by an implementation of <see cref="IResolver"/>. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    [Serializable]
    public sealed class EntityAttribute : Attribute
    {
        /// <summary>
        /// The entity name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Construct the <see cref="EntityAttribute"/>. 
        /// </summary>
        /// <param name="name">The entity name.</param>
        public EntityAttribute(string name)
        {
            SetField.NotNull(out this.Name, nameof(name), name);
        }
    }
}

namespace Microsoft.Bot.Builder.Scorables.Internals
{
    public sealed class MethodScorableFactory : IScorableFactory<IResolver, Binding>
    {
        IScorable<IResolver, Binding> IScorableFactory<IResolver, Binding>.ScorableFor(IEnumerable<MethodInfo> methods)
        {
            var specs =
                from method in methods
                from bind in InheritedAttributes.For<MethodBindAttribute>(method)
                select new { method, bind };

            var scorables = from spec in specs
                            select new MethodScorable(spec.method);

            var all = scorables.ToArray().Fold(Binding.ResolutionComparer.Instance);
            return all;
        }
    }

    /// <summary>
    /// Scorable to represent binding arguments to a method's parameters.
    /// </summary>
    [Serializable]
    public class MethodScorable : ScorableBase<IResolver, Binding, Binding>
    {
        protected readonly MethodInfo method;
        protected readonly ParameterInfo[] parameters;

        public MethodScorable(MethodInfo method)
        {
            SetField.NotNull(out this.method, nameof(method), method);
            this.parameters = this.method.GetParameters();
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.method})";
        }

        protected virtual bool TryResolveInstance(IResolver resolver, out object instance)
        {
            if (this.method.IsStatic)
            {
                instance = null;
                return true;
            }

            return resolver.TryResolve(this.method.DeclaringType, null, out instance);
        }

        protected virtual bool TryResolveArgument(IResolver resolver, ParameterInfo parameter, out object argument)
        {
            var type = parameter.ParameterType;

            var entity = parameter.GetCustomAttribute<EntityAttribute>();
            if (entity != null)
            {
                if (resolver.TryResolve(type, entity.Name, out argument))
                {
                    return true;
                }
            }

            if (resolver.TryResolve(type, parameter.Name, out argument))
            {
                return true;
            }

            return resolver.TryResolve(type, null, out argument);
        }

        protected virtual bool TryResolveArguments(IResolver resolver, out object[] arguments)
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

        protected override Task<Binding> PrepareAsync(IResolver item, CancellationToken token)
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

        protected override bool HasScore(IResolver resolver, Binding state)
        {
            return state != null;
        }

        protected override Binding GetScore(IResolver resolver, Binding state)
        {
            return state;
        }

        protected override Task PostAsync(IResolver item, Binding state, CancellationToken token)
        {
            try
            {
                return state.InvokeAsync(token);
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException(error);
            }
        }

        protected override Task DoneAsync(IResolver item, Binding state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }

    [Serializable]
    public class DelegateScorable : MethodScorable
    {
        private readonly object target;

        public DelegateScorable(Delegate lambda)
            : base(lambda.Method)
        {
            this.target = lambda.Target;
        }

        protected override bool TryResolveInstance(IResolver resolver, out object instance)
        {
            if (this.target != null)
            {
                var type = this.target.GetType();
                if (this.method.DeclaringType.IsAssignableFrom(type))
                {
                    instance = this.target;
                    return true;
                }
            }

            return base.TryResolveInstance(resolver, out instance);
        }
    }
}
