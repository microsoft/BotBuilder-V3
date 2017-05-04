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
    /// Reason for completion of Digit Collection Operation
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<DigitCollectionCompletionReason>))]
    public enum DigitCollectionCompletionReason
    {
        /// <summary>
        /// Unknown not recognized.
        /// </summary>
        Unknown,

        /// <summary>
        /// The max time period by which user is supposed to start punching in digits has elapsed.
        /// 
        /// This results in a "failed" DigitCollection Attempt
        /// </summary>
        InitialSilenceTimeout,

        /// <summary>
        /// The maximum time period between user punching in successive digits has elapsed.
        /// 
        /// This results in a "successful" DigitCollection Attempt and we return the digits collected till then.
        /// </summary>
        InterdigitTimeout,

        /// <summary>
        /// Digit collection attempt was stopped by user punching in a stop tone.
        /// 
        /// This results in a "successful" DigitCollection Attempt and we return the digits collected till then. 
        /// The stopTone(s) detected are excluded from the digits we return.
        /// </summary>
        CompletedStopToneDetected,

        /// <summary>
        /// The underlying call was terminated
        /// 
        /// This results in a "failed" DigitCollection Attempt
        /// </summary>
        CallTerminated,

        /// <summary>
        /// Misc System Failure
        /// </summary>
        TemporarySystemFailure,
    }
}