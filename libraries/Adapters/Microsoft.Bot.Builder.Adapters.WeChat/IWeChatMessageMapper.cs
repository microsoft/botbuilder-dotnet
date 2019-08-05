// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public interface IWeChatMessageMapper
    {
        Task<IActivity> ToConnectorMessage(IRequestMessageBase request);

        Task<IList<IResponseMessageBase>> ToWeChatMessages(IActivity activity);
    }
}
