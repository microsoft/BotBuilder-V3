using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.ConnectorEx
{
    /// <summary>
    /// The data persisted for ConversationReference that will be consumed for Conversation.ResumeAsync. 
    /// </summary>
    public sealed class ResumptionData
    {
        /// <summary>
        /// The locale.
        /// </summary>
        public string Locale { set; get; }

        /// <summary>
        /// The flag indicating if the ServiceUrl is trusted.
        /// </summary>
        public bool IsTrustedServiceUrl { set; get; }
    }

    /// <summary>
    /// The resumption context that is responsible for loading/persisting the <see cref="ResumptionData"/>.
    /// </summary>
    public sealed class ResumptionContext
    {

        /// <summary>
        /// The key for <see cref="ResumptionData"/> in <see cref="botDataBag"/>.
        /// </summary>
        public const string RESUMPTION_CONTEXT_KEY = "ResumptionContext";

        /// <summary>
        /// The <see cref="IBotDataBag"/> used to store the data.
        /// </summary>
        private readonly Lazy<IBotDataBag> botDataBag;

        public ResumptionContext(Func<IBotDataBag> makeBotDataBag)
        {
            SetField.CheckNull(nameof(makeBotDataBag), makeBotDataBag);
            this.botDataBag = new Lazy<IBotDataBag>(() => makeBotDataBag());
        }

        /// <summary>
        /// Load <see cref="ResumptionData"/> from <see cref="botDataBag"/>.
        /// </summary>
        public async Task<ResumptionData> LoadDataAsnyc()
        {
            ResumptionData data;
            botDataBag.Value.TryGetValue(ResumptionContext.RESUMPTION_CONTEXT_KEY, out data);
            return data;
        }

        /// <summary>
        /// Save the <paramref name="data"/> in <see cref="botDataBag"/>.
        /// </summary>
        /// <param name="data"> The <see cref="ResumptionData"/>.</param>
        public async Task SaveDataAsync(ResumptionData data)
        {
            var clonedData = new ResumptionData
            {
                Locale = data.Locale,
                IsTrustedServiceUrl = data.IsTrustedServiceUrl
            };

            botDataBag.Value.SetValue(ResumptionContext.RESUMPTION_CONTEXT_KEY, clonedData);
        }
    }

    /// <summary>
    /// Helpers for <see cref="ConversationReference"/> 
    /// </summary>
    public sealed class ConversationReferenceHelpres
    {

        /// <summary>
        /// Deserializes the GZip serialized <see cref="ConversationReference"/> using <see cref="Extensions.GZipSerialize(ConversationReference)"/>.
        /// </summary>
        /// <param name="str"> The Base64 encoded string.</param>
        /// <returns> An instance of <see cref="ConversationReference"/></returns>
        public static ConversationReference GZipDeserialize(string str)
        {
            byte[] bytes = Convert.FromBase64String(str);

            using (var stream = new MemoryStream(bytes))
            using (var gz = new GZipStream(stream, CompressionMode.Decompress))
            {
                return (ConversationReference)(new BinaryFormatter().Deserialize(gz));
            }
        }
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Creates a <see cref="ConversationReference"/> from <see cref="IAddress"/>.
        /// </summary>
        /// <param name="address"> The address.</param>
        /// <returns> The <see cref="ConversationReference"/>.</returns>
        public static ConversationReference CreateConversationReference(this IAddress address)
        {
            return new ConversationReference
            {
                Bot = new ChannelAccount { Id = address.BotId },
                ChannelId = address.ChannelId,
                User = new ChannelAccount { Id = address.UserId },
                Conversation = new ConversationAccount { Id = address.ConversationId },
                ServiceUrl = address.ServiceUrl
            };
        }

        /// <summary>
        /// Creates a <see cref="ConversationReference"/> from <see cref="ResumptionCookie"/>.
        /// </summary>
        /// <param name="resumptionCookie"> The resumption cookie.</param>
        /// <returns> The <see cref="ConversationReference"/>.</returns>
        public static ConversationReference CreateConversationReference(this ResumptionCookie resumptionCookie)
        {
            return new ConversationReference
            {
                Bot = new ChannelAccount { Id = resumptionCookie.Address.BotId, Name = resumptionCookie.UserName },
                ChannelId = resumptionCookie.Address.ChannelId,
                User = new ChannelAccount { Id = resumptionCookie.Address.UserId, Name = resumptionCookie.UserName },
                Conversation = new ConversationAccount { Id = resumptionCookie.Address.ConversationId, IsGroup = resumptionCookie.IsGroup },
                ServiceUrl = resumptionCookie.Address.ServiceUrl
            };
        }

        /// <summary>
        /// Creates a <see cref="ConversationReference"/> from <see cref="IActivity"/>.
        /// </summary>
        /// <param name="activity"> The <see cref="IActivity"/>  posted to bot.</param>
        /// <returns> The <see cref="ConversationReference"/>.</returns>
        public static ConversationReference CreateConversationReference(this IActivity activity)
        {
            return new ConversationReference
            {
                Bot = new ChannelAccount { Id = activity.Recipient.Id, Name = activity.Recipient.Name },
                ChannelId = activity.ChannelId,
                User = new ChannelAccount { Id = activity.From.Id, Name = activity.From.Name },
                Conversation = new ConversationAccount { Id = activity.Conversation.Id, IsGroup = activity.Conversation.IsGroup, Name = activity.Conversation.Name },
                ServiceUrl = activity.ServiceUrl
            };
        }

        /// <summary>
        /// Binary serializes <see cref="ConversationReference"/> using <see cref="GZipStream"/>.
        /// </summary>
        /// <param name="conversationReference"> The resumption cookie.</param>
        /// <returns> A Base64 encoded string.</returns>
        public static string GZipSerialize(this ConversationReference conversationReference)
        {
            using (var cmpStream = new MemoryStream())
            using (var stream = new GZipStream(cmpStream, CompressionMode.Compress))
            {
                new BinaryFormatter().Serialize(stream, conversationReference);
                stream.Close();
                return Convert.ToBase64String(cmpStream.ToArray());
            }
        }
    }
}
