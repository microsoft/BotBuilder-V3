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

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Internals.Scorables
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ScorableOrderAttribute : Attribute
    {
        private readonly int order;
        public int Order => this.order;
        public ScorableOrderAttribute(int order)
        {
            this.order = order;
        }
    }

    // http://blog.ploeh.dk/2014/06/13/passive-attributes/
    public interface IScorableFactory<in Item, out Score>
    {
        IScorable<Item, Score> ScorableFor(IEnumerable<MethodInfo> methods);
    }

    public sealed class OrderScorableFactory<Item, Score> : IScorableFactory<Item, Score>
    {
        private readonly IEnumerable<IScorableFactory<Item, Score>> factories;
        public OrderScorableFactory(IEnumerable<IScorableFactory<Item, Score>> factories)
        {
            SetField.NotNull(out this.factories, nameof(factories), factories);
        }
        public OrderScorableFactory(params IScorableFactory<Item, Score>[] factories)
            : this((IEnumerable<IScorableFactory<Item, Score>>)factories)
        {
        }
        IScorable<Item, Score> IScorableFactory<Item, Score>.ScorableFor(IEnumerable<MethodInfo> methods)
        {
            var levels = from method in methods
                         // note, this is non-deterministic across executions, which seems lame
                         let defaultOrder = method.Name.GetHashCode()
                         let orders = InheritedAttributes.For<ScorableOrderAttribute>(method).Select(order => order.Order).DefaultIfEmpty(defaultOrder)
                         from order in orders
                         group method by order into g
                         orderby g.Key
                         select g;

            var scorables = from level in levels
                            from factory in this.factories
                            let scorable = factory.ScorableFor(level)
                            //where scorable != null
                            select scorable;

            var winner = scorables.ToArray().First();
            return winner;
        }
    }
}
