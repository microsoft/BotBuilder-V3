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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is part of the "recognize" action. If the customer wants to collect digits - this needs to be specified.
    /// Ex: enter 5 digit zip code followed by pound sign.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class CollectDigits
    {
        /// <summary>
        /// Maximum number of digits expected
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public uint? MaxNumberOfDtmfs { get; set; }

        /// <summary>
        /// Stop tones specified to end collection
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public IEnumerable<char> StopTones { get; set; }

        public void Validate()
        {
            bool stopTonesSet = this.StopTones != null && this.StopTones.Any();
            Utils.AssertArgument(
                this.MaxNumberOfDtmfs.GetValueOrDefault() > 0 || stopTonesSet,
                "For CollectDigits either stopTones or maxNumberOfDigits or both must be specified");

            if (this.MaxNumberOfDtmfs.HasValue)
            {
                Utils.AssertArgument(this.MaxNumberOfDtmfs.Value >= MinValues.NumberOfDtmfsExpected && this.MaxNumberOfDtmfs.Value <= MaxValues.NumberOfDtmfsExpected,
                    "MaxNumberOfDtmfs has to be specified in the range of {0} - {1}", MinValues.NumberOfDtmfsExpected, MaxValues.NumberOfDtmfsExpected);
            }

            if (stopTonesSet)
            {
                ValidDtmfs.Validate(this.StopTones);
            }
        }
    }
}
