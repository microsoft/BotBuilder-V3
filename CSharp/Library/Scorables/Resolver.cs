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

using Autofac;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Scorables.Internals
{
    /// <summary>
    /// Allow the resolution of values based on type and optionally tag.
    /// </summary>
    /// <remarks>
    /// The tag should be restrictive to services registered with that tag.
    /// </remarks>
    public interface IResolver
    {
        bool TryResolve(Type type, object tag, out object value);
    }

    public delegate bool TryResolve(Type type, object tag, out object value);

    public static partial class Extensions
    {
        public static bool TryResolve<T>(this IResolver resolver, object tag, out T value)
        {
            object inner;
            if (resolver.TryResolve(typeof(T), tag, out inner))
            {
                value = (T)inner;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }
    }

    public sealed class NullResolver : IResolver
    {
        public static readonly IResolver Instance = new NullResolver();

        private NullResolver()
        {
        }

        bool IResolver.TryResolve(Type type, object tag, out object value)
        {
            value = null;
            return false;
        }
    }

    public sealed class NoneResolver : IResolver
    {
        public static readonly IResolver Instance = new NoneResolver();

        private NoneResolver()
        {
        }

        public static readonly object BoxedToken = CancellationToken.None;

        bool IResolver.TryResolve(Type type, object tag, out object value)
        {
            if (typeof(CancellationToken).IsAssignableFrom(type))
            {
                value = BoxedToken;
                return true;
            }

            value = null;
            return false;
        }
    }

    public abstract class DelegatingResolver : IResolver
    {
        protected readonly IResolver inner;

        protected DelegatingResolver(IResolver inner)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }

        public virtual bool TryResolve(Type type, object tag, out object value)
        {
            return inner.TryResolve(type, tag, out value);
        }
    }

    public sealed class EnumResolver : DelegatingResolver
    {
        public EnumResolver(IResolver inner)
            : base(inner)
        {
        }

        public override bool TryResolve(Type type, object tag, out object value)
        {
            if (type.IsEnum)
            {
                var name = tag as string;
                if (name != null)
                {
                    if (Enum.IsDefined(type, name))
                    {
                        value = Enum.Parse(type, name);
                        return true;
                    }
                }
            }

            return base.TryResolve(type, tag, out value);
        }
    }

    public sealed class ArrayResolver : DelegatingResolver
    {
        private readonly object[] services;

        public ArrayResolver(IResolver inner, params object[] services)
            : base(inner)
        {
            SetField.NotNull(out this.services, nameof(services), services);
        }

        public override bool TryResolve(Type type, object tag, out object value)
        {
            if (tag == null)
            {
                for (int index = 0; index < this.services.Length; ++index)
                {
                    var service = this.services[index];
                    if (service != null)
                    {
                        var serviceType = service.GetType();
                        if (type.IsAssignableFrom(serviceType))
                        {
                            value = service;
                            return true;
                        }
                    }
                }
            }

            return base.TryResolve(type, tag, out value);
        }
    }

    /// <summary>
    /// A resolver to recover C# type information from Activity schema types.
    /// </summary>
    public sealed class ActivityResolver : DelegatingResolver
    {
        public ActivityResolver(IResolver inner)
            : base(inner)
        {
        }

        public static readonly IReadOnlyDictionary<string, Type> TypeByName = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { ActivityTypes.ContactRelationUpdate, typeof(IContactRelationUpdateActivity) },
            { ActivityTypes.ConversationUpdate, typeof(IConversationUpdateActivity) },
            { ActivityTypes.DeleteUserData, typeof(IActivity) },
            { ActivityTypes.Message, typeof(IMessageActivity) },
            { ActivityTypes.Ping, typeof(IActivity) },
            { ActivityTypes.Trigger, typeof(ITriggerActivity) },
            { ActivityTypes.Typing, typeof(ITypingActivity) },
        };

        public override bool TryResolve(Type type, object tag, out object value)
        {
            if (tag == null)
            {
                // if type is Activity, we're not delegating to the inner IResolver.
                if (typeof(IActivity).IsAssignableFrom(type))
                {
                    // if we have a registered IActivity
                    IActivity activity;
                    if (this.inner.TryResolve<IActivity>(tag, out activity))
                    {
                        // then make sure the IActivity.Type allows the desired type
                        Type allowedType;
                        if (TypeByName.TryGetValue(activity.Type, out allowedType))
                        {
                            if (type.IsAssignableFrom(allowedType))
                            {
                                // and make sure the actual CLR type also allows the desired type
                                // (this is true most of the time since Activity implements all of the interfaces)
                                Type clrType = activity.GetType();
                                if (allowedType.IsAssignableFrom(clrType))
                                {
                                    value = activity;
                                    return true;
                                }
                            }
                        }
                    }

                    // otherwise we were asking for IActivity and it wasn't assignable from the IActivity.Type
                    value = null;
                    return false;
                }
            }

            // delegate to the inner for all remaining type resolutions
            return base.TryResolve(type, tag, out value);
        }
    }

    public sealed class TriggerValueResolver : DelegatingResolver
    {
        public TriggerValueResolver(IResolver inner)
            : base(inner)
        {
        }

        public override bool TryResolve(Type type, object tag, out object value)
        {
            ITriggerActivity trigger;
            if (this.inner.TryResolve(tag, out trigger))
            {
                var triggerValue = trigger.Value;
                if (triggerValue != null)
                {
                    var triggerValueType = triggerValue.GetType();
                    if (type.IsAssignableFrom(triggerValueType))
                    {
                        value = triggerValue;
                        return true;
                    }
                }
            }

            return base.TryResolve(type, tag, out value);
        }
    }

    public sealed class AutofacResolver : DelegatingResolver
    {
        private readonly IComponentContext context;

        public AutofacResolver(IComponentContext context, IResolver inner)
            : base(inner)
        {
            SetField.NotNull(out this.context, nameof(context), context);
        }

        public override bool TryResolve(Type type, object tag, out object value)
        {
            if (tag != null && this.context.TryResolveKeyed(tag, type, out value))
            {
                return true;
            }
            else if (this.context.TryResolve(type, out value))
            {
                return true;
            }

            return base.TryResolve(type, tag, out value);
        }
    }
}
