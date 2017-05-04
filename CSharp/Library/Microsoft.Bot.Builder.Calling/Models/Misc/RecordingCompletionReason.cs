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

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Misc
{
    /// <summary>
    /// Reason for completion of Recording Operation
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<RecordingCompletionReason>))]
    public enum RecordingCompletionReason
    {
        /// <summary>
        /// Unknown not recognized.
        /// </summary>
        Unknown,

        /// <summary>
        /// The maximum initial silence that can be tolerated had been reached
        /// 
        /// This results in a "failed" Recording Attempt
        /// </summary>
        InitialSilenceTimeout,

        /// <summary>
        /// The maximum duration that can be allowed for recording had been reached
        /// 
        /// This results in a "successful" Recording Attempt
        /// </summary>
        MaxRecordingTimeout,

        /// <summary>
        /// Recording was completed as detected by silence after a talk spurt
        /// 
        /// This results in a "successful" Recording Attempt
        /// </summary>
        CompletedSilenceDetected,

        /// <summary>
        /// Recording was completed by user punching in a stop tone
        /// 
        /// This results in a "successful" Recording Attempt
        /// </summary>
        CompletedStopToneDetected,

        /// <summary>
        /// The underlying call was terminated
        /// 
        /// This results in a "successful" Recording Attempt if there were any bytes recorded
        /// </summary>
        CallTerminated,

        /// <summary>
        /// Misc System Failure
        /// </summary>
        TemporarySystemFailure,
    }
}