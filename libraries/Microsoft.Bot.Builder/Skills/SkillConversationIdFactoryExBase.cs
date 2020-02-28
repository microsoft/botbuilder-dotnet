// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// Defines the interface of a factory that is used to create unique conversation IDs for skill conversations.
    /// </summary>
    public abstract class SkillConversationIdFactoryExBase : SkillConversationIdFactoryBase
    {
        /// <summary>
        /// Creates a conversation id for a skill conversation.
        /// </summary>
        /// <param name="originatingAudience">The oauth audience scope, used during token retrieval. (Either https://api.botframework.com or bot app id.)</param>
        /// <param name="fromBotId">The id of the parent bot that is messaging the skill.</param>
        /// <param name="activity">The activity which will be sent to the skill.</param>
        /// <param name="botFrameworkSkill">The skill to create the conversation Id for.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A unique conversation ID used to communicate with the skill.</returns>
        /// <remarks>
        /// It should be possible to use the returned string on a request URL and it should not contain special characters. 
        /// </remarks>
        public abstract Task<string> CreateSkillConversationIdAsync(string originatingAudience, string fromBotId, Activity activity, BotFrameworkSkill botFrameworkSkill, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the <see cref="ConversationReference"/> and originatingAudience used during <see cref="CreateSkillConversationIdAsync"/> for a skillConversationId.
        /// </summary>
        /// <param name="skillConversationId">A skill conversationId created using <see cref="CreateSkillConversationIdAsync"/>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The caller's <see cref="ConversationReference"/> for a skillConversationId, with originatingAudience. Null if not found.</returns>
        public abstract Task<(ConversationReference, string)> GetConversationReferenceWithAudienceAsync(string skillConversationId, CancellationToken cancellationToken);

        [Obsolete("Method is deprecated, please use SkillConversationIdFactoryExBase.CreateSkillConversationIdAsync with audience.", true)]
        public override Task<string> CreateSkillConversationIdAsync(ConversationReference conversationReference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("CreateSkillConversationIdAsync without audience is deprecated.");
        }

        [Obsolete("Method is deprecated, please use SkillConversationIdFactoryExBase.GetConversationReferenceWithAudienceAsync", true)]
        public override Task<ConversationReference> GetConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("GetConversationReferenceAsync is deprecated.");
        }
    }
}
