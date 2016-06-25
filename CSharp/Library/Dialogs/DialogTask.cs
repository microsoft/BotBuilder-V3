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
    public sealed class DialogTask : IDialogStack, IPostToBot
    {
        private readonly Func<IDialogContext> makeContext;
        private readonly IStore<IFiberLoop<DialogTask>> store;
        private readonly IFiberLoop<DialogTask> fiber;
        private readonly Frames frames;
        public DialogTask(Func<IDialogContext> makeContext, IStore<IFiberLoop<DialogTask>> store)
        {
            SetField.NotNull(out this.makeContext, nameof(makeContext), makeContext);
            SetField.NotNull(out this.store, nameof(store), store);
            this.store.TryLoad(out this.fiber);
            this.frames = new Frames(this);
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

                await this.start(task.makeContext());
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
                await this.resume(task.makeContext(), item);
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

        async Task IDialogStack.Forward<R, T>(IDialog<R> child, ResumeAfter<R> resume, T item, CancellationToken token)
        {
            IDialogStack stack = this;
            stack.Call(child, resume);
            await stack.PollAsync(token);
            await (this as IPostToBot).PostAsync(item, token);
        }

        void IDialogStack.Done<R>(R value)
        {
            this.wait = this.fiber.Done(value);
        }

        void IDialogStack.Fail(Exception error)
        {
            this.wait = this.fiber.Fail(error);
        }

        void IDialogStack.Wait<R>(ResumeAfter<R> resume)
        {
            this.wait = this.fiber.Wait<DialogTask, R>(ToRest(resume));
        }

        async Task IDialogStack.PollAsync(CancellationToken token)
        {
            await this.fiber.PollAsync(this);
        }

        void IDialogStack.Reset()
        {
            this.store.Reset();
            this.store.Flush();
            this.fiber.Reset();
        }

        async Task IPostToBot.PostAsync<T>(T item, CancellationToken token)
        {
            try
            {
                this.fiber.Post(item);
                await this.fiber.PollAsync(this);
            }
            catch
            {
                this.store.Reset();
                throw;
            }
            finally
            {
                this.store.Save(this.fiber);
                this.store.Flush(); 
            }
        }
    }

    public sealed class ReactiveDialogTask : IPostToBot
    {
        private readonly IPostToBot inner;
        private readonly IDialogStack stack;
        private readonly IStore<IFiberLoop<DialogTask>> store;
        private readonly Func<IDialog<object>> makeRoot;

        public ReactiveDialogTask(IPostToBot inner, IDialogStack stack, IStore<IFiberLoop<DialogTask>> store, Func<IDialog<object>> makeRoot)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.stack, nameof(stack), stack);
            SetField.NotNull(out this.store, nameof(store), store);
            SetField.NotNull(out this.makeRoot, nameof(makeRoot), makeRoot);
        }

        async Task IPostToBot.PostAsync<T>(T item, CancellationToken token)
        {
            try
            {
                if (this.stack.Frames.Count == 0)
                {
                    var root = this.makeRoot();
                    var loop = root.Loop();
                    this.stack.Call(loop, null);
                    await this.stack.PollAsync(token);
                }

                await this.inner.PostAsync(item, token);
            }
            catch
            {
                this.store.Reset();
                throw;
            }
        }
    }


    public struct LocalizedScope : IDisposable
    {
        private readonly CultureInfo previousCulture;
        private readonly CultureInfo previousUICulture;

        public LocalizedScope(string locale)
        {
            this.previousCulture = Thread.CurrentThread.CurrentCulture;
            this.previousUICulture = Thread.CurrentThread.CurrentUICulture;

            if (!string.IsNullOrWhiteSpace(locale))
            {
                CultureInfo found = null;
                try
                {
                    found = CultureInfo.GetCultureInfo(locale);
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

    public sealed class LocalizedDialogTask : IPostToBot
    {
        private readonly IPostToBot inner;

        public LocalizedDialogTask(IPostToBot inner)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }

        async Task IPostToBot.PostAsync<T>(T item, CancellationToken token)
        {
            using (new LocalizedScope((item as IMessageActivity)?.Locale))
            {
                await this.inner.PostAsync<T>(item, token);
            }
        }
    }

    public sealed class PersistentDialogTask : IPostToBot
    {
        private readonly Lazy<IPostToBot> inner;
        private readonly IConnectorClient client;
        private readonly IMessageActivity message;
        private readonly IBotToUser botToUser;
        private readonly IBotData botData;

        public PersistentDialogTask(Func<IPostToBot> makeInner, IMessageActivity message, IConnectorClient client, IBotToUser botToUser, IBotData botData)
        {
            SetField.NotNull(out this.message, nameof(message), message);
            SetField.NotNull(out this.client, nameof(client), client);
            SetField.NotNull(out this.botToUser, nameof(botToUser), botToUser);
            SetField.NotNull(out this.botData, nameof(botData), botData);
            SetField.CheckNull(nameof(makeInner), makeInner);
            this.inner = new Lazy<IPostToBot>(() => makeInner());
        }

        async Task IPostToBot.PostAsync<T>(T item, CancellationToken token)
        {
            await botData.LoadAsync(token);
            try
            {
                await this.inner.Value.PostAsync<T>(item, token);
            }
            finally
            {
                await PersistBotData(token: token);
            }
        }

        private async Task PersistBotData(bool ignoreETag = true, CancellationToken token = default(CancellationToken))
        {
            await botData.FlushAsync(token);
        }
    }
}
