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
    /// A <see cref="BotFrameworkHttpClient"/>specialized for Skills that encapsulates Conversation ID generation.
    /// </summary>
    public class SkillHttpClient : BotFrameworkHttpClient
    {
        private readonly SkillConversationIdFactoryBase _conversationIdFactory;

        public SkillHttpClient(HttpClient httpClient, ICredentialProvider credentialProvider, SkillConversationIdFactoryBase conversationIdFactory, IChannelProvider channelProvider = null, ILogger logger = null)
            : base(httpClient, credentialProvider, channelProvider, logger)
        {
            _conversationIdFactory = conversationIdFactory;
        }

        public async Task<InvokeResponse> PostActivityAsync(string fromBotId, BotFrameworkSkill toSkill, Uri callbackUrl, Activity activity, CancellationToken cancellationToken)
        {
            var skillConversationId = await _conversationIdFactory.CreateSkillConversationIdAsync(activity.GetConversationReference(), cancellationToken).ConfigureAwait(false);
            return await PostActivityAsync(fromBotId, toSkill.AppId, toSkill.SkillEndpoint, callbackUrl, skillConversationId, activity, cancellationToken).ConfigureAwait(false);
        }
    }
}
