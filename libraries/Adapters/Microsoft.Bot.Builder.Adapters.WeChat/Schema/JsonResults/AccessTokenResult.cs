// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResults
{
    public class AccessTokenResult : WeChatJsonResult
    {
        [JsonProperty("access_token")]
        public string Token { get; set; }

        [JsonProperty("expires_in")]
        public int ExpireIn { get; set; }
    }
}
