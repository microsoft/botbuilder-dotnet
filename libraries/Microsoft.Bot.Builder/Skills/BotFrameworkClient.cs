// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// A Bot Framework client.
    /// </summary>
    public abstract class BotFrameworkClient
    {
        /// <summary>
        /// Forwards an activity to a skill (bot).
        /// </summary>
        /// <remarks>NOTE: Forwarding an activity to a skill will flush UserState and ConversationState changes so that skill has accurate state.</remarks>
        /// <param name="fromBotId">The MicrosoftAppId of the bot sending the activity.</param>
        /// <param name="toBotId">The MicrosoftAppId of the bot receiving the activity.</param>
        /// <param name="toUrl">The URL of the bot receiving the activity.</param>
        /// <param name="serviceUrl">The callback Url for the skill host.</param>
        /// <param name="conversationId">A conversation ID to use for the conversation with the skill.</param>
        /// <param name="activity">The <see cref="Activity"/> to send to forward.</param>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>Async task with optional invokeResponse.</returns>
        public abstract Task<InvokeResponse> PostActivityAsync(string fromBotId, string toBotId, Uri toUrl, Uri serviceUrl, string conversationId, Activity activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Forwards an activity to a skill (bot).
        /// </summary>
        /// <remarks>NOTE: Forwarding an activity to a skill will flush UserState and ConversationState changes so that skill has accurate state.</remarks>
        /// <typeparam name="T">The type of body in the InvokeResponse.</typeparam>
        /// <param name="fromBotId">The MicrosoftAppId of the bot sending the activity.</param>
        /// <param name="toBotId">The MicrosoftAppId of the bot receiving the activity.</param>
        /// <param name="toUrl">The URL of the bot receiving the activity.</param>
        /// <param name="serviceUrl">The callback Url for the skill host.</param>
        /// <param name="conversationId">A conversation ID to use for the conversation with the skill.</param>
        /// <param name="activity">The <see cref="Activity"/> to send to forward.</param>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>Async task with optional invokeResponse.</returns>
        public abstract Task<InvokeResponse<T>> PostActivityAsync<T>(string fromBotId, string toBotId, Uri toUrl, Uri serviceUrl, string conversationId, Activity activity, CancellationToken cancellationToken = default);
    }
}
