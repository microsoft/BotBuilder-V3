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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;

using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    public sealed class DialogContextStore : IDialogContextStore
    {
        private readonly IFormatter formatter;
        private readonly IBotData botData;
        private readonly string key;
        public DialogContextStore(IFormatter formatter, IBotData botData, string key)
        {
            SetField.NotNull(out this.formatter, nameof(formatter), formatter);
            SetField.NotNull(out this.botData, nameof(botData), botData);
            SetField.NotNull(out this.key, nameof(key), key);
        }

        public IBotDataBag Bag {  get { return this.botData.PerUserInConversationData; } }

        bool IDialogContextStore.TryLoad(out IDialogContextInternal context)
        {
            byte[] blobOld;
            bool found = this.Bag.TryGetValue(this.key, out blobOld);
            if (found)
            {
                using (var streamOld = new MemoryStream(blobOld))
                using (var gzipOld = new GZipStream(streamOld, CompressionMode.Decompress))
                {
                    context = (IDialogContextInternal)this.formatter.Deserialize(gzipOld);
                    return true;
                }
            }

            context = null;
            return false;
        }

        void IDialogContextStore.Save(IDialogContextInternal context)
        {
            byte[] blobNew;
            using (var streamNew = new MemoryStream())
            using (var gzipNew = new GZipStream(streamNew, CompressionMode.Compress))
            {
                formatter.Serialize(gzipNew, context);
                gzipNew.Close();
                blobNew = streamNew.ToArray();
            }

            this.Bag.SetValue(this.key, blobNew);
        }
    }

    public sealed class ErrorResilientDialogContextStore : IDialogContextStore
    {
        private readonly IDialogContextStore store;
        public ErrorResilientDialogContextStore(IDialogContextStore store)
        {
            SetField.NotNull(out this.store, nameof(store), store);
        }
        bool IDialogContextStore.TryLoad(out IDialogContextInternal context)
        {
            try
            {
                return this.store.TryLoad(out context);
            }
            catch (Exception)
            {
                // exception in loading the serialized context data
                context = null;
                return false;
            }
        }

        void IDialogContextStore.Save(IDialogContextInternal context)
        {
            this.store.Save(context);
        }
    }

    public sealed class DialogContextFactory : IDialogContextStore
    {
        private readonly IDialogContextStore store;
        private readonly IFrameFactory frames;
        private readonly IBotToUser botToUser;
        private readonly IBotData botData;

        public DialogContextFactory(IDialogContextStore store, IFrameFactory frames, IBotToUser botToUser, IBotData botData)
        {
            SetField.NotNull(out this.store, nameof(store), store);
            SetField.NotNull(out this.frames, nameof(frames), frames);
            SetField.NotNull(out this.botToUser, nameof(botToUser), botToUser);
            SetField.NotNull(out this.botData, nameof(botData), botData);
        }

        bool IDialogContextStore.TryLoad(out IDialogContextInternal context)
        {
            if (store.TryLoad(out context))
            {
                return true;
            }
            else
            {
                IFiberLoop fiber = new Fiber(frames);
                context = new DialogContext(botToUser, botData, fiber);
                return false;
            }
        }

        void IDialogContextStore.Save(IDialogContextInternal context)
        {
            this.store.Save(context);
        }
    }
}
