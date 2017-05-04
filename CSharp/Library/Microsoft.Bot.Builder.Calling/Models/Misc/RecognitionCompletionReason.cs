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
    /// Reason for completion of Recognition(speech/dtmf) Operation
    /// </summary>
    [JsonConverter(typeof(StringEnumConverterWithDefault<RecognitionCompletionReason>))]
    public enum RecognitionCompletionReason
    {
        /// <summary>
        /// Unknown not recognized.
        /// </summary>
        Unknown,

        /// <summary>
        /// The maximum initial silence that can be tolerated had been reached
        /// 
        /// This results in a "failed" Recognition Attempt
        /// </summary>
        InitialSilenceTimeout,

        /// <summary>
        /// The Recognition completed because the user punched in wrong dtmf which was not amongst the possible 
        /// choices. 
        /// 
        /// We would only look for dtmfs when dtmf recognition is requested. Thus for pure speech menus, this
        /// completion reason would never be possible.
        /// 
        /// This results in a "failed" Recognition Attempt
        /// </summary>
        IncorrectDtmf,

        /// <summary>
        /// The maximum time period between user punching in successive digits has elapsed.
        /// 
        /// We would only look for dtmfs when dtmf recognition is requested. Thus for pure speech menus, this
        /// completion reason would never be possible.
        /// 
        /// This results in a "failed" Recognition Attempt.
        /// </summary>
        InterdigitTimeout,

        /// <summary>
        /// The recognition successfully matched a Grammar option
        /// </summary>
        SpeechOptionMatched,

        /// <summary>
        /// The recognition successfully matched a Dtmf option
        /// </summary>
        DtmfOptionMatched,

        /// <summary>
        /// The underlying call was terminated
        /// 
        /// This results in a "failed" Recognition Attempt
        /// </summary>
        CallTerminated,

        /// <summary>
        /// Misc System Failure
        /// </summary>
        TemporarySystemFailure,
    }
}