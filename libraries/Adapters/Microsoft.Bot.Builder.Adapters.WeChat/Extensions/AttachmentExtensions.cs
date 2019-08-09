// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Extensions
{
    /// <summary>
    /// Attachment Extensions to easily convert attachment type to card, etc.
    /// </summary>
    internal static class AttachmentExtensions
    {
        public static T ContentAs<T>(this Attachment attachment)
        {
            if (attachment.Content == null)
            {
                return default(T);
            }
            else if (typeof(T).IsValueType)
            {
                return (T)Convert.ChangeType(attachment.Content, typeof(T), CultureInfo.InvariantCulture);
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
