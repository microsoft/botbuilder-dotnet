// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResults
{
    public class UploadPersistentMediaResult : UploadMediaResult
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
