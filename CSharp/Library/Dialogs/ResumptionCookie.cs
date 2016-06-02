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
        /// The user address.
        /// </summary>
        [JsonProperty(PropertyName = "userAddress")]
        public string UserAddress { set; get; }

        /// <summary>
        /// The user channelId.
        /// </summary>
        [JsonProperty(PropertyName = "userChannelId")]
        public string UserChannelId { set; get; }

        /// <summary>
        /// The bot Id. 
        /// </summary>
        [JsonProperty(PropertyName = "botId")]
        public string BotId { set; get; }
        /// <summary>
        /// The bot Address.
        /// </summary>
        [JsonProperty(PropertyName = "botAddress")]
        public string BotAddress { set; get; }
        /// <summary>
        /// The bot channel Id.
        /// </summary>
        [JsonProperty(PropertyName = "botChannelId")]
        public string BotChannelId { set; get; }

        /// <summary>
        /// The Id of the conversation that will be resumed.
        /// </summary>
        [JsonProperty(PropertyName = "conversationId")]
        public string ConversationId { set; get; }

        /// <summary>
        /// The language of message.
        /// </summary>
        [JsonProperty(PropertyName = "language")]
        public string Language { set; get; }

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
        /// <param name="language"> The language of the message.</param>
        public ResumptionCookie(string userId, string botId, string conversationId, string channelId, string language = "en")
        {
            SetField.CheckNull(nameof(userId), userId);
            SetField.CheckNull(nameof(botId), botId);
            SetField.CheckNull(nameof(conversationId), conversationId);
            SetField.CheckNull(nameof(channelId), channelId);
            SetField.CheckNull(nameof(language), language);
            this.UserId = userId;
            this.BotId = botId;
            this.ConversationId = conversationId;
            this.UserChannelId = channelId;
            this.Language = language;
        }

        /// <summary>
        /// Creates an instance of resumption cookie form a <see cref="Connector.Message"/>
        /// </summary>
        /// <param name="msg"> The message.</param>
        public ResumptionCookie(Message msg)
        {
            UserId = msg.From?.Id;
            UserAddress = msg.From?.Address;
            UserChannelId = msg.From?.ChannelId;
            BotId = msg.To?.Id;
            BotAddress = msg.To?.Address;
            BotChannelId = msg.To?.ChannelId;
            ConversationId = msg.ConversationId;
            Language = msg.Language;
        }

        /// <summary>
        /// Creates a message from the resumption cookie.
        /// </summary>
        /// <returns> The message that can be sent to bot based on the resumption cookie</returns>
        public Message GetMessage()
        {
            return new Message
            {
                Id = Guid.NewGuid().ToString(),
                To = new ChannelAccount
                {
                    Id = this.BotId,
                    IsBot = true,
                    Address = this.BotAddress,
                    ChannelId = this.BotChannelId
                },
                ConversationId = this.ConversationId,
                From = new ChannelAccount
                {
                    Id = this.UserId,
                    IsBot = false,
                    Address = this.UserAddress,
                    ChannelId = this.UserChannelId
                },
                Language = this.Language
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
