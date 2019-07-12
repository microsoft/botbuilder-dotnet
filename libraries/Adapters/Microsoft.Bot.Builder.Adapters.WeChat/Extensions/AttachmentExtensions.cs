using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BotSchema = Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    /// <summary>
    /// Attachment Extensions to easily convert attachemnt type to card, etc.
    /// </summary>
    public static partial class AttachmentExtensions
    {
        public static T ContentAs<T>(this BotSchema.Attachment attachment)
        {
            if (attachment.Content == null)
            {
                return default(T);
            }
            else if (typeof(T).IsValueType)
            {
                return (T)Convert.ChangeType(attachment.Content, typeof(T));
            }
            else if (attachment.Content is T)
            {
                return (T)attachment.Content;
            }
            else if (typeof(T) == typeof(byte[]))
            {
                return (T)(object)Convert.FromBase64String(attachment.Content.ToString());
            }
            else if (attachment.Content is string)
            {
                return JsonConvert.DeserializeObject<T>((string)attachment.Content);
            }

            return (T)((JObject)attachment.Content).ToObject<T>();
        }
    }
}
