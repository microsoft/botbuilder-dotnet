// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder
{
    public interface ICredentialTokenProviderWithEmulator : ICredentialTokenProvider
    {
        /// <summary>
        /// Get the raw signin link to be sent to the user for signin for a connection name, using customized AppCredentials.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="oAuthAppCredentials">AppCredentials for OAuth.</param>
        /// <param name="connectionName">Name of the auth connection to use.</param>
        /// <param name="emulatorUrl">Url of the emulator service.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the raw signin link.</remarks>
        Task<string> GetOauthSignInLinkAsync(ITurnContext turnContext, AppCredentials oAuthAppCredentials, string connectionName, string emulatorUrl, CancellationToken cancellationToken);
    }
}
