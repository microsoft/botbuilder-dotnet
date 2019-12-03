// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// Defines the interface of a factory that is used to create unique conversation IDs for skill conversations.
    /// </summary>
    public interface ISkillConversationIdFactory
    {
        /// <summary>
        /// Creates a unique conversation ID based on a unique identifier and the skill's caller serviceUrl.
        /// </summary>
        /// <param name="callerConversationId">The skill's caller conversationId.</param>
        /// <param name="serviceUrl">The skill's caller serviceUrl for the activity being sent.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A unique conversation ID used to communicate with the skill.</returns>
        /// <remarks>
        /// It should be possible to use the returned string on a request URL and it should not contain special characters. 
        /// </remarks>
        Task<string> CreateSkillConversationIdAsync(string callerConversationId, string serviceUrl, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the original conversationId and ServiceUrl created using <see cref="CreateSkillConversationIdAsync"/> for a skillConversationId.
        /// </summary>
        /// <param name="skillConversationId">A conversationId created using <see cref="CreateSkillConversationIdAsync"/>.</param>
        /// /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>the original conversationId and ServiceUrl for a skillConversationId.</returns>
        Task<(string conversationId, string serviceUrl)> GetConversationInfoAsync(string skillConversationId, CancellationToken cancellationToken);
    }
}
