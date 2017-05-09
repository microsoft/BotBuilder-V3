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
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// This is part of the "recognize" action. If the customer wants to speech/dtmf choice based recognition - this needs to be specified.
    /// Ex: say "Sales" or enter 1 for Sales department
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class RecognitionOption
    {
        /// <summary>
        /// Name of the choice. Once a choice matches, this name is conveyed back to the customer in the outcome.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Speech variations which form the grammar for the choice. 
        /// Ex: Name : "Yes" , SpeechVariation : {"Yes", "yeah", "ya", "yo" }
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public IEnumerable<string> SpeechVariation { get; set; }

        /// <summary>
        /// Dtmf variations for the choice. 
        /// Ex: Name : "Yes" , DtmfVariation : {'1'}
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public char? DtmfVariation { get; set; }

        public void Validate()
        {
            Utils.AssertArgument(!String.IsNullOrWhiteSpace(this.Name), "Choice 'Name' must be set to a valid non-empty value");

            bool speechVariationSet = this.SpeechVariation != null && this.SpeechVariation.Any();
            bool dtmfVarationSet = this.DtmfVariation != null;

            Utils.AssertArgument(speechVariationSet || dtmfVarationSet, "SpeechVariation or DtmfVariation or both must be set");

            if (speechVariationSet)
            {
                foreach (string s in SpeechVariation)
                {
                    Utils.AssertArgument(!String.IsNullOrWhiteSpace(s), "Null or empty choice cannot be set for speech variation");
                }

                Utils.AssertArgument(this.SpeechVariation.Count() <= MaxValues.NumberOfSpeechVariations, "Number of speech variations specified cannot exceed : {0}", MaxValues.NumberOfSpeechVariations);
            }

            if (dtmfVarationSet)
            {
                ValidDtmfs.Validate(this.DtmfVariation.Value);
            }
        }

        public static void Validate(IEnumerable<RecognitionOption> choices)
        {
            Utils.AssertArgument(choices != null, "choices list cannot be null");
            Utils.AssertArgument(choices.Any(), "choices list cannot be empty");
            HashSet<string> speechChoice = new HashSet<string>();
            HashSet<char> dtmfChoice = new HashSet<char>();
            foreach (var choice in choices)
            {
                Utils.AssertArgument(choice != null, "choice cannot be null");
                choice.Validate();
                if (choice.DtmfVariation.HasValue)
                {
                    char c = choice.DtmfVariation.Value;
                    Utils.AssertArgument(!dtmfChoice.Contains(c), "Dtmf choices must be uniquely specified across all recognition options");
                    dtmfChoice.Add(c);
                }
                if (choice.SpeechVariation != null)
                {
                    foreach (string sc in choice.SpeechVariation)
                    {
                        Utils.AssertArgument(!speechChoice.Contains(sc), "Speech choices must be uniquely specified across all recognition options");
                        speechChoice.Add(sc);
                    }
                }
            }
        }
    }
}
