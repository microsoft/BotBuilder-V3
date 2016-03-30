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
    public interface IDialogContextStore
    {
        bool TryLoad(IBotDataBag bag, string key, out IDialogContext context);
        void Save(IDialogContext context, IBotDataBag bag, string key);
    }

    public sealed class DialogContextStore : IDialogContextStore
    {
        private readonly IFormatter formatter;
        public DialogContextStore(IFormatter formatter)
        {
            SetField.NotNull(out this.formatter, nameof(formatter), formatter);
        }

        bool IDialogContextStore.TryLoad(IBotDataBag bag, string key, out IDialogContext context)
        {
            byte[] blobOld;
            bool found = bag.TryGetValue(key, out blobOld);
            if (found)
            {
                using (var streamOld = new MemoryStream(blobOld))
                using (var gzipOld = new GZipStream(streamOld, CompressionMode.Decompress))
                {
                    context = (IDialogContext)this.formatter.Deserialize(gzipOld);
                    return true;
                }
            }

            context = null;
            return false;
        }

        void IDialogContextStore.Save(IDialogContext context, IBotDataBag bag, string key)
        {
            byte[] blobNew;
            using (var streamNew = new MemoryStream())
            using (var gzipNew = new GZipStream(streamNew, CompressionMode.Compress))
            {
                formatter.Serialize(gzipNew, context);
                gzipNew.Close();
                blobNew = streamNew.ToArray();
            }

            bag.SetValue(key, blobNew);
        }
    }
}
