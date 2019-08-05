// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResult
{
    public class UploadImgResult : WeChatJsonResult
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
