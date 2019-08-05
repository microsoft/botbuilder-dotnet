// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event
{
    public interface IRequestMessageEventBase : IRequestMessageBase
    {
        string Event { get; }
    }
}
