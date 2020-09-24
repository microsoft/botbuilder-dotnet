// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Integration
{
    /// <summary>
    /// An interface that defines the contract between web service integration pieces and the bot adapter.
    /// </summary>
    public interface IAdapterIntegration
    {
        /// <summary>
        /// Creates a turn context and runs the middleware pipeline for an incoming activity.
        /// </summary>
        /// <param name="authHeader">The HTTP authentication header of the request.</param>
        /// <param name="activity">The incoming activity.</param>
        /// <param name="callback">The code to run at the end of the adapter's middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute. If the activity type
        /// was 'Invoke' and the corresponding key (channelId + activityId) was found
        /// then an InvokeResponse is returned, otherwise null is returned.</returns>
        Task<InvokeResponse> ProcessActivityAsync(
          string authHeader,
          Activity activity,
          BotCallbackHandler callback,
          CancellationToken cancellationToken);

        /// <summary>
        /// Sends a proactive message to a conversation.
        /// </summary>
        /// <param name="botId">The application ID of the bot. This parameter is ignored in
        /// single tenant the Adapters (Console, Test, etc) but is critical to the BotFrameworkAdapter
        /// which is multi-tenant aware. </param>
        /// <param name="reference">A reference to the conversation to continue.</param>
        /// <param name="callback">The method to call for the resulting bot turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Call this method to proactively send a message to a conversation.
        /// Most _channels require a user to initiate a conversation with a bot
        /// before the bot can send activities to the user.</remarks>
        /// <seealso cref="BotAdapter.ContinueConversationAsync(string, ConversationReference, BotCallbackHandler, CancellationToken)"/>
        Task ContinueConversationAsync(
            string botId,
            ConversationReference reference,
            BotCallbackHandler callback,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
