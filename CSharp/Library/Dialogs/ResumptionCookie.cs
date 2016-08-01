// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
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
//

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// The resumption cookie that can be used to resume a conversation with a user. 
    /// </summary>
    [Serializable]
    public class ResumptionCookie
    {
        /// <summary>
        /// The user Id.
        /// </summary>
        [JsonProperty(PropertyName = "userId")]
        public string UserId { set; get; }

        /// <summary>
        /// The user name.
        /// </summary>
        [JsonProperty(PropertyName = "userName")]
        public string UserName { set; get; }

        /// <summary>
        /// The channelId.
        /// </summary>
        [JsonProperty(PropertyName = "channelId")]
        public string ChannelId { set; get; }

        /// <summary>
        /// The bot Id. 
        /// </summary>
        [JsonProperty(PropertyName = "botId")]
        public string BotId { set; get; }

        /// <summary>
        /// The service url.
        /// </summary>
        [JsonProperty(PropertyName = "serviceUrl")]
        public string ServiceUrl { set; get; }

        /// <summary>
        /// True if the <see cref="ServiceUrl"/> is trusted; False otherwise.
        /// </summary>
        /// <remarks> <see cref="Conversation.ResumeAsync{T}(ResumptionCookie, T, System.Threading.CancellationToken)"/> adds 
        /// the host of the <see cref="ServiceUrl"/> to <see cref="MicrosoftAppCredentials.TrustedHostNames"/> if this flag is True.
        /// </remarks>
        public bool IsTrustedServiceUrl { protected set; get; }

        /// <summary>
        /// The IsGroup flag for conversation.
        /// </summary>
        [JsonProperty(PropertyName = "isGroup")]
        public bool IsGroup { set; get; }

        /// <summary>
        /// The Id of the conversation that will be resumed.
        /// </summary>
        [JsonProperty(PropertyName = "conversationId")]
        public string ConversationId { set; get; }

        /// <summary>
        /// The locale of message.
        /// </summary>
        [JsonProperty(PropertyName = "locale")]
        public string Locale { set; get; }

        public ResumptionCookie()
        {
        }

        /// <summary>
        /// Creates an instance of the resumption cookie. 
        /// </summary>
        /// <param name="userId"> The user Id.</param>
        /// <param name="botId"> The bot Id.</param>
        /// <param name="conversationId"> The conversation Id.</param>
        /// <param name="channelId"> The channel Id of the conversation.</param>
        /// <param name="serviceUrl"> The service url of the conversation.</param>
        /// <param name="locale"> The locale of the message.</param>
        public ResumptionCookie(string userId, string botId, string conversationId, string channelId, string serviceUrl, string locale = "en")
        {
            SetField.CheckNull(nameof(userId), userId);
            SetField.CheckNull(nameof(botId), botId);
            SetField.CheckNull(nameof(conversationId), conversationId);
            SetField.CheckNull(nameof(channelId), channelId);
            SetField.CheckNull(nameof(serviceUrl), serviceUrl);
            SetField.CheckNull(nameof(locale), locale);
            this.UserId = userId;
            this.BotId = botId;
            this.ConversationId = conversationId;
            this.ChannelId = channelId;
            this.ServiceUrl = serviceUrl;
            this.IsTrustedServiceUrl = MicrosoftAppCredentials.IsTrustedServiceUrl(serviceUrl);
            this.Locale = locale;
        }

        /// <summary>
        /// Creates an instance of resumption cookie form a <see cref="Connector.IMessageActivity"/>
        /// </summary>
        /// <param name="msg"> The message.</param>
        public ResumptionCookie(IMessageActivity msg)
        {
            UserId = msg.From?.Id;
            UserName = msg.From?.Name;
            ChannelId = msg.ChannelId;
            ServiceUrl = msg.ServiceUrl;
            IsTrustedServiceUrl = MicrosoftAppCredentials.IsTrustedServiceUrl(msg.ServiceUrl);
            BotId = msg.Recipient?.Id;
            ConversationId = msg.Conversation?.Id;
            var isGroup =  msg.Conversation?.IsGroup;
            IsGroup = isGroup.HasValue && isGroup.Value;
            Locale = msg.Locale;
        }

        /// <summary>
        /// Creates a message from the resumption cookie.
        /// </summary>
        /// <returns> The message that can be sent to bot based on the resumption cookie</returns>
        public IMessageActivity GetMessage()
        {
            return new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Recipient = new ChannelAccount
                {
                    Id = this.BotId
                },
                ChannelId = this.ChannelId, 
                ServiceUrl = this.ServiceUrl,
                Conversation = new ConversationAccount
                {
                    Id = this.ConversationId, 
                    IsGroup = this.IsGroup
                },
                From = new ChannelAccount
                {
                    Id = this.UserId,
                    Name = this.UserName
                },
                Locale = this.Locale
            };
        }

        /// <summary>
        /// Deserializes the GZip serialized <see cref="ResumptionCookie"/> using <see cref="Extensions.GZipSerialize(ResumptionCookie)"/>.
        /// </summary>
        /// <param name="str"> The Base64 encoded string.</param>
        /// <returns> An instance of <see cref="ResumptionCookie"/></returns>
        public static ResumptionCookie GZipDeserialize(string str)
        {
            byte[] bytes = Convert.FromBase64String(str);

            using (var stream = new MemoryStream(bytes))
            using (var gz = new GZipStream(stream, CompressionMode.Decompress))
            {
                return (ResumptionCookie)(new BinaryFormatter().Deserialize(gz));
            }
        }
    }

    public partial class Extensions
    {
        /// <summary>
        /// Binary serializes <see cref="ResumptionCookie"/> using <see cref="GZipStream"/>.
        /// </summary>
        /// <param name="resumptionCookie"> The resumption cookie.</param>
        /// <returns> A Base64 encoded string.</returns>
        public static string GZipSerialize(this ResumptionCookie resumptionCookie)
        {
            using (var cmpStream = new MemoryStream())
            using (var stream = new GZipStream(cmpStream, CompressionMode.Compress))
            {
                new BinaryFormatter().Serialize(stream, resumptionCookie);
                stream.Close();
                return Convert.ToBase64String(cmpStream.ToArray());
            }
        }
    }
}
