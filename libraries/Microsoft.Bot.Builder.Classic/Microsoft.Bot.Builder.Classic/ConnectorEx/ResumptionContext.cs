using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.ConnectorEx
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
        /// <param name="token"> The cancellation token.</param>
        public async Task<ResumptionData> LoadDataAsync(CancellationToken token)
        {
            ResumptionData data;
            botDataBag.Value.TryGetValue(ResumptionContext.RESUMPTION_CONTEXT_KEY, out data);
            return data;
        }

        /// <summary>
        /// Save the <paramref name="data"/> in <see cref="botDataBag"/>.
        /// </summary>
        /// <param name="data"> The <see cref="ResumptionData"/>.</param>
        /// <param name="token"> The cancellation token.</param>
        public async Task SaveDataAsync(ResumptionData data, CancellationToken token)
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
    public sealed class ConversationReferenceHelpers
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
        public static ConversationReference ToConversationReference(this IAddress address)
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

#pragma warning disable CS0618 //disable obsolete warning for this helper.
        /// <summary>
        /// Creates a <see cref="ConversationReference"/> from <see cref="ResumptionCookie"/>.
        /// </summary>
        /// <param name="resumptionCookie"> The resumption cookie.</param>
        /// <returns> The <see cref="ConversationReference"/>.</returns>
        public static ConversationReference ToConversationReference(this ResumptionCookie resumptionCookie)
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
#pragma warning restore CS0618

        /// <summary>
        /// Creates a <see cref="ConversationReference"/> from <see cref="IActivity"/>.
        /// </summary>
        /// <param name="activity"> The <see cref="IActivity"/>  posted to bot.</param>
        /// <returns> The <see cref="ConversationReference"/>.</returns>
        public static ConversationReference ToConversationReference(this IActivity activity)
        {
            return new ConversationReference
            {
                ActivityId = activity.Id,
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
