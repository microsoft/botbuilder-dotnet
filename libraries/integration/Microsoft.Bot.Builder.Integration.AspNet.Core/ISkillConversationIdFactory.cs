// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Defines the interface of a factory that is used to create unique conversation IDs for skill conversations.
    /// </summary>
    public interface ISkillConversationIdFactory
    {
        /// <summary>
        /// Creates a unique conversation ID based on a unique identifier and the skill's caller serviceUrl.
        /// </summary>
        /// <param name="conversationId">A unique identifier for the conversation with the skill.</param>
        /// <param name="serviceUrl">The skill's caller service Url.</param>
        /// <returns>A unique conversation ID used to communicate with the skill.</returns>
        /// <remarks>
        /// It should be possible to use the returned string on a request URL and it should not contain special characters. 
        /// </remarks>
        string CreateSkillConversationId(string conversationId, string serviceUrl);

        /// <summary>
        /// Decodes a conversationId string created using <see cref="CreateSkillConversationId"/>.
        /// </summary>
        /// <param name="encodedConversationId">An encoded conversationId.</param>
        /// <returns>The original identifier used to create the create the conversation ID and the serviceUrl.</returns>
        (string conversationId, string serviceUrl) GetConversationInfo(string encodedConversationId);
    }
}
