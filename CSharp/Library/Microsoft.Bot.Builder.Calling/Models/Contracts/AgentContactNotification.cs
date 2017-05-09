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

namespace Microsoft.Bot.Builder.Calling.ObjectModel.Contracts
{
    /// <summary>
    /// Message type for notifying agents that a user has added or removed
    /// them.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class AgentContactNotification : BaseMessage
    {
        public const string TypeName = "AgentContactNotification";
        public const string UserDisplayNameKey = "fromUserDisplayName";
        public const string LcidKey = "fromUserLcid";
        public const string ActionKey = "action";
        public const string ClientCountryKey = "clientCountry";
        public const string ClientUiLanguageKey = "clientUiLanguage";
        public const string ClientVersionKey = "clientVersion";
        public const string ContactAddAction = "add";
        public const string ContactRemoveAction = "remove";

        public AgentContactNotification()
            : base(AgentContactNotification.TypeName)
        {
        }

        [JsonProperty(PropertyName = UserDisplayNameKey, Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = LcidKey, Required = Required.Default)]
        public int Lcid { get; set; }

        [JsonProperty(PropertyName = ActionKey, Required = Required.Always)]
        public string Action { get; set; }

        [JsonProperty(PropertyName = ClientCountryKey, Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string ClientCountry { get; set; }

        [JsonProperty(PropertyName = ClientUiLanguageKey, Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string ClientUiLanguage { get; set; }

        [JsonProperty(PropertyName = ClientVersionKey, Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string ClientVersion { get; set; }

        protected override void ValidateInternal()
        {
            VerifyPropertyExists(this.DisplayName, "DisplayName");
            VerifyPropertyExists(this.Action, "Action");
        }
    }
}
