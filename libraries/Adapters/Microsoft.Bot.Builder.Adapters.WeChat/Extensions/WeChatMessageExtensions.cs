// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Extensions
{
    public static class WeChatMessageExtensions
    {
        public static void SetProperties(this ResponseMessage responseMessage, IActivity activity)
        {
            responseMessage.FromUserName = activity.From.Id;
            responseMessage.ToUserName = activity.Recipient.Id;
            responseMessage.CreateTime = activity.Timestamp.HasValue ? activity.Timestamp.Value.ToUnixTimeSeconds() : DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
