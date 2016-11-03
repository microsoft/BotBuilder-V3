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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    public interface IDialogTaskManager
    {
        /// <summary>
        /// Loads the <see cref="DialogTasks"/> from <see cref="IBotData.PrivateConversationData"/>.
        /// </summary>
        /// <param name="token"> The cancellation token.</param>
        Task TryLoadDialogTasks(CancellationToken token);

        /// <summary>
        /// Flushes the <see cref="IDialogTask"/> in <see cref="DialogTasks"/>
        /// </summary>
        /// <param name="token"> The cancellation token.</param>
        Task FlushDialogTasks(CancellationToken token);

        /// <summary>
        /// The list of <see cref="IDialogTask"/>
        /// </summary>
        IReadOnlyList<IDialogTask> DialogTasks { get; }

        /// <summary>
        /// Creates a new <see cref="IDialogTask"/> and add it to <see cref="DialogTasks"/>
        /// </summary>
        IDialogTask CreateDialogTask(CancellationToken token);
    }

    public sealed class DialogTaskManager : IDialogTaskManager
    {
        private readonly string blobKeyPrefix;
        private readonly IBotData botData;
        private readonly IBotToUser botToUser;
        private readonly IStackStoreFactory<DialogTask> stackStoreFactory;

        private List<DialogTask> dialogTasks;
        private readonly object createLock = new object();


        public DialogTaskManager(string blobKeyPrefix, IBotData botData,
            IStackStoreFactory<DialogTask> stackStoreFactory, IBotToUser botToUser)
        {
            SetField.NotNull(out this.blobKeyPrefix, nameof(blobKeyPrefix), blobKeyPrefix);
            SetField.NotNull(out this.botData, nameof(botData), botData);
            SetField.NotNull(out this.botToUser, nameof(botToUser), botToUser);
            SetField.NotNull(out this.stackStoreFactory, nameof(stackStoreFactory), stackStoreFactory);
        }

        async Task IDialogTaskManager.TryLoadDialogTasks(CancellationToken token)
        {
            if (this.dialogTasks == null)
            {
                // load all dialog tasks. By default it loads/creates the default dialog task 
                // which will be used by ReactiveDialogTask
                this.dialogTasks = new List<DialogTask>();
                do
                {
                    IDialogTaskManager dialogTaskManager = this;
                    dialogTaskManager.CreateDialogTask(token);
                } while (
                    this.botData.PrivateConversationData.ContainsKey(this.GetCurrentTaskBlobKey(this.dialogTasks.Count)));
            }
        }

        async Task IDialogTaskManager.FlushDialogTasks(CancellationToken token)
        {
            foreach (var dialogTask in this.dialogTasks)
            {
                dialogTask.Store.Flush();
            }
        }


        public IReadOnlyList<IDialogTask> DialogTasks
        {
            get { return this.dialogTasks; }
        }

        IDialogTask IDialogTaskManager.CreateDialogTask(CancellationToken token)
        {
            lock (createLock)
            {
                var dialogTask = this.CreateDialogTask(this.dialogTasks.Count, token);
                this.dialogTasks.Add(dialogTask);
                return dialogTask;
            }
        }

        private DialogTask CreateDialogTask(int idx, CancellationToken token)
        {
            IDialogContext context = default(IDialogContext);
            var dialogTask = new DialogTask((cancellation) => context, stackStoreFactory.StoreFrom(this.GetCurrentTaskBlobKey(idx)));
            context = new DialogContext(this.botToUser, this.botData, dialogTask, token);
            return dialogTask;
        }

        private string GetCurrentTaskBlobKey(int idx)
        {
            return idx == 0 ? this.blobKeyPrefix : this.blobKeyPrefix + idx;
        }
    }
}
