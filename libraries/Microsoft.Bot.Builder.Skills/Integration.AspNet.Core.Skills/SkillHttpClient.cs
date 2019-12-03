// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Skills
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

        public async Task<InvokeResponse> PostActivityAsync(string fromBotId, BotFrameworkSkill skill, Uri callbackUrl, Activity activity, CancellationToken cancellationToken)
        {
            var skillConversationId = await _conversationIdFactory.CreateSkillConversationIdAsync(activity.Conversation.Id, activity.ServiceUrl, cancellationToken).ConfigureAwait(false);
            return await PostActivityAsync(fromBotId, skill.AppId, skill.SkillEndpoint, callbackUrl, skillConversationId, activity, cancellationToken).ConfigureAwait(false);
        }
    }
}
