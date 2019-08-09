// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResults
{
    public class UploadTemporaryMediaResult : UploadMediaResult
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("thumb_media_id")]
        public string ThumbMediaId { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

        // By wechat description temporary media will expired in 3 days
        public override bool Expired()
        {
            var expiredTime = CreatedAt + (3 * 24 * 60 * 60);
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() >= expiredTime;
        }
    }
}
