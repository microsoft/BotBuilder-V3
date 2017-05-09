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

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is a helper class for validating dtmfs specified in strings
    /// </summary>
    public static class ValidDtmfs
    {
        /// <summary>
        /// list of valid dtmfs
        /// </summary>
        public static readonly Dictionary<char, int> ValidDtmfSet = new Dictionary<char, int>()
        {
            {'0', 0},
            {'1', 1},
            {'2', 2},
            {'3', 3},
            {'4', 4},
            {'5', 5},
            {'6', 6},
            {'7', 7},
            {'8', 8},
            {'9', 9},
            {'*', 10},
            {'#', 11},
            {'A', 12},
            {'B', 13},
            {'C', 14},
            {'D', 15},
        };

        public static void Validate(char digit)
        {
            Utils.AssertArgument(ValidDtmfSet.ContainsKey(digit), "'{0}' is not a valid dtmf", digit);
        }

        public static void Validate(IEnumerable<char> variations)
        {
            Utils.AssertArgument(variations != null, "Specified digit list is null");
            int count = variations.Count();
            Utils.AssertArgument(count > 0, "Specified digit list is empty");
            Utils.AssertArgument(count <= MaxValues.NumberOfStopTones, "Number of stop tones specified cannot exceed : {0}", MaxValues.NumberOfStopTones);
            foreach (char c in variations)
            {
                Validate(c);
            }
        }
    }
}
