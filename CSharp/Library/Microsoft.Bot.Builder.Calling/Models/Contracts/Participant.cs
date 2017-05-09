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
    /// This class describes a participant.
    /// This can be a participant in any modality in a 2 or multi-party conversation
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Participant : IEquatable<Participant>
    {
        /// <summary>
        /// MRI of the participant .ex : 2:+14258828080 or '8:alice' 
        /// </summary>
        [JsonProperty(Required = Required.Always, Order = -2)]
        public string Identity { get; set; }

        /// <summary>
        /// Display name of participant if received from the controllers
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Participant language. This property is optional and only passed if
        /// participant language is known.
        /// Examples of valid values are null, "en", "en-US".
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string LanguageId { get; set; }

        /// <summary>
        /// Is this participant the originator of the conversation
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public bool Originator { get; set; }

        public bool Equals(Participant other)
        {
            if (other == null)
            {
                return false;
            }

            if (Identity.Equals(other.Identity, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            Participant participant = obj as Participant;
            if (participant != null)
            {
                return Equals(participant);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Identity.GetHashCode();
        }

        public void Validate()
        {
            Utils.AssertArgument(!String.IsNullOrWhiteSpace(this.Identity), "Identity of participant must be specified");
        }

        public static void Validate(IEnumerable<Participant> participants)
        {
            Utils.AssertArgument(participants != null, "participant list cannot be null");
            Utils.AssertArgument(participants.Any(), "participant list cannot be empty");
            foreach (Participant participant in participants)
            {
                Utils.AssertArgument(participant != null, "participant cannot be null");
                participant.Validate();
            }
        }
    }
}
