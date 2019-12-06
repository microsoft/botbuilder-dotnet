// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        public abstract Task<string> CreateSkillConversationIdAsync(ConversationReference conversationReference, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the <see cref="ConversationReference"/> created using <see cref="CreateSkillConversationIdAsync"/> for a skillConversationId.
        /// </summary>
        /// <param name="skillConversationId">A skill conversationId created using <see cref="CreateSkillConversationIdAsync"/>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The caller's <see cref="ConversationReference"/> for a skillConversationId. null if not found.</returns>
        public abstract Task<ConversationReference> GetConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a <see cref="ConversationReference"/>.
        /// </summary>
        /// <param name="skillConversationId">A skill conversationId created using <see cref="CreateSkillConversationIdAsync"/>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public abstract Task DeleteConversationReferenceAsync(string skillConversationId, CancellationToken cancellationToken);
    }
}
