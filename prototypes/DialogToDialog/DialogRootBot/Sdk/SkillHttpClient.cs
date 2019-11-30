// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace DialogRootBot.Sdk
{
    /// <summary>
    /// EXPERIMENTAL: WIP a BotFrameworkHttpClient specialized for Skills that encapsulates Conversation ID generation.
    /// </summary>
    public class SkillHttpClient : BotFrameworkHttpClient
    {
        private readonly ISkillConversationIdFactory _conversationIdFactory;

        public SkillHttpClient(HttpClient httpClient, ICredentialProvider credentialProvider, ISkillConversationIdFactory conversationIdFactory, IChannelProvider channelProvider = null, ILogger logger = null)
            : base(httpClient, credentialProvider, channelProvider, logger)
        {
            _conversationIdFactory = conversationIdFactory;
        }

        public Task<InvokeResponse> PostActivityAsync(string fromBotId, string toBotId, Uri toUrl, Uri serviceUrl, Activity activity, CancellationToken cancellationToken)
        {
            var skillConversationId = _conversationIdFactory.CreateSkillConversationId(activity.Conversation.Id, activity.ServiceUrl);
            return PostActivityAsync(fromBotId, toBotId, toUrl, serviceUrl, skillConversationId, activity, cancellationToken);
        }
    }
}
