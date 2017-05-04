// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
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

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    public static class MultiPartConstants
    {
        /// <summary>
        /// content disposition name to use for the binary recorded audio (in a multipart response)
        /// </summary>
        public static readonly string RecordingContentDispositionName = "recordedAudio";

        /// <summary>
        /// content disposition name to use for the result object (in a multipart response)
        /// </summary>
        public static readonly string ResultContentDispositionName = "conversationResult";

        /// <summary>
        /// mime type for wav
        /// </summary>
        public static readonly string WavMimeType = "audio/wav";

        /// <summary>
        /// mime type for wma
        /// </summary>
        public static readonly string WmaMimeType = "audio/x-ms-wma";

        /// <summary>
        /// mime type for mp3
        /// </summary>
        public static readonly string Mp3MimeType = "audio/mpeg";
    }
}