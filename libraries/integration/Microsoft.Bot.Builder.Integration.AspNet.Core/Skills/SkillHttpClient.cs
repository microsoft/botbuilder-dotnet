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

        /// <summary>
        /// Uses the SkillConversationIdFactory to create or retrieve a Skill Conversation Id, and sends the activity.
        /// </summary>
        /// <typeparam name="T">The type of body in the InvokeResponse.</typeparam>
        /// <param name="originatingAudience">The oauth audience scope, used during token retrieval. (Either https://api.botframework.com or bot app id.)</param>
        /// <param name="fromBotId">The MicrosoftAppId of the bot sending the activity.</param>
        /// <param name="toSkill">The skill to create the conversation Id for.</param>
        /// <param name="callbackUrl">The callback Url for the skill host.</param>
        /// <param name="activity">The activity to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Async task with invokeResponse.</returns>
        public async Task<InvokeResponse<T>> PostActivityAsync<T>(string originatingAudience, string fromBotId, BotFrameworkSkill toSkill, Uri callbackUrl, Activity activity, CancellationToken cancellationToken)
        {
            string skillConversationId;
            try
            {
                var options = new SkillConversationIdFactoryOptions
                {
                    FromBotOAuthScope = originatingAudience,
                    FromBotId = fromBotId,
                    Activity = activity,
                    BotFrameworkSkill = toSkill
                };
                skillConversationId = await _conversationIdFactory.CreateSkillConversationIdAsync(options, cancellationToken).ConfigureAwait(false);
            }
            catch (NotImplementedException)
            {
                // Attempt to create the ID using deprecated method.
#pragma warning disable 618 // Keeping this for backward compat, this catch should be removed when the deprecated method is removed.
                skillConversationId = await _conversationIdFactory.CreateSkillConversationIdAsync(activity.GetConversationReference(), cancellationToken).ConfigureAwait(false);
#pragma warning restore 618
            }

            return await PostActivityAsync<T>(fromBotId, toSkill.AppId, toSkill.SkillEndpoint, callbackUrl, skillConversationId, activity, cancellationToken).ConfigureAwait(false);
        }

        public async Task<InvokeResponse> PostActivityAsync(string fromBotId, BotFrameworkSkill toSkill, Uri callbackUrl, Activity activity, CancellationToken cancellationToken)
        {
            return await PostActivityAsync<object>(fromBotId, toSkill, callbackUrl, activity, cancellationToken).ConfigureAwait(false);
        }

        public async Task<InvokeResponse<T>> PostActivityAsync<T>(string fromBotId, BotFrameworkSkill toSkill, Uri callbackUrl, Activity activity, CancellationToken cancellationToken)
        {
            var originatingAudience = ChannelProvider != null && ChannelProvider.IsGovernment() ? GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope : AuthenticationConstants.ToChannelFromBotOAuthScope;
            return await PostActivityAsync<T>(originatingAudience, fromBotId, toSkill, callbackUrl, activity, cancellationToken).ConfigureAwait(false);
        }
    }
}
