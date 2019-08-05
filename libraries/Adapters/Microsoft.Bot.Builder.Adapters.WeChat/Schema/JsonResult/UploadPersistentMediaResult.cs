// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResult
{
    public class UploadPersistentMediaResult : WeChatJsonResult
    {
        [JsonProperty("media_id")]
        public string MediaId { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
