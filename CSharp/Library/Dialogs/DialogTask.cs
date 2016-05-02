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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    public sealed class DialogTask : IDialogTask
    {
        private readonly Func<IDialogContext> factory;
        private readonly IStore<IFiberLoop<DialogTask>> store;
        private readonly IFiberLoop<DialogTask> fiber;
        private readonly Frames frames;
        public DialogTask(Func<IDialogContext> factory, IStore<IFiberLoop<DialogTask>> store)
        {
            SetField.NotNull(out this.factory, nameof(factory), factory);
            SetField.NotNull(out this.store, nameof(store), store);
            this.store.TryLoad(out this.fiber);
            this.frames = new Frames(this);
        }

        void IDialogTask.Reset()
        {
            this.store.Reset();
        }

        void IDialogTask.Save()
        {
            this.store.Save(this.fiber);
        }

        private IWait<DialogTask> wait;

        public interface IThunk
        {
            Delegate Method { get; }
        }

        [Serializable]
        private sealed class ThunkStart : IThunk
        {
            private readonly StartAsync start;
            public ThunkStart(StartAsync start)
            {
                SetField.NotNull(out this.start, nameof(start), start);
            }

            Delegate IThunk.Method
            {
                get
                {
                    return this.start;
                }
            }

            public async Task<IWait<DialogTask>> Rest(IFiber<DialogTask> fiber, DialogTask task, IItem<object> item)
            {
                var result = await item;
                if (result != null)
                {
                    throw new ArgumentException(nameof(item));
                }

                await this.start(task.factory());
                return task.wait;
            }
        }

        [Serializable]
        private sealed class ThunkResume<T> : IThunk
        {
            private readonly ResumeAfter<T> resume;
            public ThunkResume(ResumeAfter<T> resume)
            {
                SetField.NotNull(out this.resume, nameof(resume), resume);
            }

            Delegate IThunk.Method
            {
                get
                {
                    return this.resume;
                }
            }

            public async Task<IWait<DialogTask>> Rest(IFiber<DialogTask> fiber, DialogTask task, IItem<T> item)
            {
                await this.resume(task.factory(), item);
                return task.wait;
            }
        }

        internal Rest<DialogTask, object> ToRest(StartAsync start)
        {
            var thunk = new ThunkStart(start);
            return thunk.Rest;
        }

        internal Rest<DialogTask, T> ToRest<T>(ResumeAfter<T> resume)
        {
            var thunk = new ThunkResume<T>(resume);
            return thunk.Rest;
        }

        private sealed class Frames : IReadOnlyList<Delegate>
        {
            private readonly DialogTask task;
            public Frames(DialogTask task)
            {
                SetField.NotNull(out this.task, nameof(task), task);
            }

            int IReadOnlyCollection<Delegate>.Count
            {
                get
                {
                    return this.task.fiber.Frames.Count;
                }
            }

            public Delegate Map(int ordinal)
            {
                var frames = this.task.fiber.Frames;
                int index = frames.Count - ordinal - 1;
                var frame = frames[index];
                var wait = frame.Wait;
                var rest = wait.Rest;
                var thunk = (IThunk) rest.Target;
                return thunk.Method;
            }

            Delegate IReadOnlyList<Delegate>.this[int index]
            {
                get
                {
                    return this.Map(index);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                IEnumerable<Delegate> enumerable = this;
                return enumerable.GetEnumerator();
            }

            IEnumerator<Delegate> IEnumerable<Delegate>.GetEnumerator()
            {
                var frames = this.task.fiber.Frames;
                for (int index = 0; index < frames.Count; ++index)
                {
                    yield return this.Map(index);
                }
            }
        }

        IReadOnlyList<Delegate> IDialogStack.Frames
        {
            get
            {
                return this.frames;
            }
        }

        void IDialogStack.Call<R>(IDialog<R> child, ResumeAfter<R> resume)
        {
            var callRest = ToRest(child.StartAsync);
            if (resume != null)
            {
                var doneRest = ToRest(resume);
                this.wait = this.fiber.Call<DialogTask, object, R>(callRest, null, doneRest);
            }
            else
            {
                this.wait = this.fiber.Call<DialogTask, object>(callRest, null);
            }
        }

        void IDialogStack.Done<R>(R value)
        {
            this.wait = this.fiber.Done(value);
        }

        void IDialogStack.Fail(Exception error)
        {
            this.wait = this.fiber.Fail(error);
        }

        void IDialogStack.Wait(ResumeAfter<Message> resume)
        {
            this.wait = this.fiber.Wait<DialogTask, Message>(ToRest(resume));
        }

        async Task IDialogTask.PollAsync()
        {
            await this.fiber.PollAsync(this);
        }

        async Task IPostToBot.PostAsync<T>(T item, CancellationToken cancellationToken)
        {
            this.fiber.Post(item);
            await this.fiber.PollAsync(this);
        }
    }

    public struct LocalizedScope : IDisposable
    {
        private readonly CultureInfo previousCulture;
        private readonly CultureInfo previousUICulture;

        public LocalizedScope(string language)
        {
            this.previousCulture = Thread.CurrentThread.CurrentCulture;
            this.previousUICulture = Thread.CurrentThread.CurrentUICulture;

            if (!string.IsNullOrWhiteSpace(language))
            {
                CultureInfo found = null;
                try
                {
                    found = CultureInfo.GetCultureInfo(language);
                }
                catch (CultureNotFoundException)
                {
                }

                if (found != null)
                {
                    Thread.CurrentThread.CurrentCulture = found;
                    Thread.CurrentThread.CurrentUICulture = found;
                }
            }
        }

        public void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = previousCulture;
            Thread.CurrentThread.CurrentUICulture = previousUICulture;
        }
    }

    public sealed class LocalizedDialogTask : DelegatingDialogTask
    {
        public LocalizedDialogTask(IDialogTask inner)
            : base(inner)
        {
        }

        public override async Task PostAsync<T>(T item, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (new LocalizedScope((item as Message)?.Language))
            {
                await base.PostAsync<T>(item, cancellationToken);
            }
        }
    }
}
