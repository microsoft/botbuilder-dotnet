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
    public abstract class SkillConversationIdFactoryBase
    {
        /// <summary>
        /// Creates a conversation ID for a skill conversation based on the caller's <see cref="ConversationReference"/>.
        /// </summary>
        /// <param name="conversationReference">The skill's caller <see cref="ConversationReference"/>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A unique conversation ID used to communicate with the skill.</returns>
        /// <remarks>
        /// It should be possible to use the returned string on a request URL and it should not contain special characters. 
        /// </remarks>
        [Obsolete("Method is deprecated, please use CreateSkillConversationIdAsync() with SkillConversationIdFactoryOptions instead.", false)]
        public virtual Task<string> CreateSkillConversationIdAsync(ConversationReference conversationReference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a conversation id for a skill conversation.
        /// </summary>
        /// <param name="options">A <see cref="SkillConversationIdFactoryOptions"/> instance containing parameters for creating the conversation ID.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A unique conversation ID used to communicate with the skill.</returns>
        /// <remarks>
        /// It should be possible to use the returned string on a request URL and it should not contain special characters. 
        /// </remarks>
        public virtual Task<string> CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions options, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the <see cref="ConversationReference"/> created using <see cref="CreateSkillConversationIdAsync(Microsoft.Bot.Schema.ConversationReference,System.Threading.CancellationToken)"/> for a skillConversationId.
        /// </summary>
        /// <param name="skillConversationId">A skill conversationId created using <see cref="CreateSkillConversationIdAsync(Microsoft.Bot.Schema.ConversationReference,System.Threading.CancellationToken)"/>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The caller's <see cref="ConversationReference"/> for a skillConversationId. null if not found.</returns>
        [Obsolete("Method is deprecated, please use GetSkillConversationReferenceAsync() instead.", false)]
        public virtual Task<ConversationReference> GetConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the <see cref="SkillConversationReference"/> used during <see cref="CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions,System.Threading.CancellationToken)"/> for a skillConversationId.
        /// </summary>
        /// <param name="skillConversationId">A skill conversationId created using <see cref="CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions,System.Threading.CancellationToken)"/>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The caller's <see cref="ConversationReference"/> for a skillConversationId, with originatingAudience. Null if not found.</returns>
        public virtual Task<SkillConversationReference> GetSkillConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes a <see cref="ConversationReference"/>.
        /// </summary>
        /// <param name="skillConversationId">A skill conversationId created using <see cref="CreateSkillConversationIdAsync(SkillConversationIdFactoryOptions,System.Threading.CancellationToken)"/>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public abstract Task DeleteConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken);
    }
}
