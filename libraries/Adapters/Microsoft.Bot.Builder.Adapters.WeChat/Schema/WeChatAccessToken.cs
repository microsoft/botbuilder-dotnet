// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public class WeChatAccessToken
    {
        public WeChatAccessToken()
        {
            ExpireTime = DateTimeOffset.MinValue;
        }

        public string AppId { get; set; }

        public string Token { get; set; }

        public string Secret { get; set; }

        public DateTimeOffset ExpireTime { get; set; }
    }
}
