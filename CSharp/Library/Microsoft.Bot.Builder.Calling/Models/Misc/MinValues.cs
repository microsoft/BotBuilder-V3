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

using System;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    public static class MinValues
    {
        /// <summary>
        /// Minimum allowed recording duration.
        /// </summary>
        public static readonly TimeSpan RecordingDuration = TimeSpan.FromSeconds(2.0);

        ///<summary>
        /// Minimum allowed silence once the user has started speaking before we conclude 
        /// the user is done recording.
        /// </summary>
        public static readonly TimeSpan SilenceTimeout = TimeSpan.FromSeconds(0.0);

        /// <summary>
        /// Minimum initial silence allowed from the time we start the operation 
        /// before we timeout and fail the operation. 
        /// </summary>
        public static readonly TimeSpan InitialSilenceTimeout = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Mamimum allowed time between digits if we are doing dtmf based choice recognition or CollectDigits recognition
        /// </summary>
        public static readonly TimeSpan InterdigitTimeout = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Maximum number of digits expected
        /// </summary>
        public static readonly uint NumberOfDtmfsExpected = 1;
    }
}