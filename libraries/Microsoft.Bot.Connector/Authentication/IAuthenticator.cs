// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Contract for authentication classes that retrieve authentication tokens.
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// Acquires the security token.
        /// </summary>
        /// <param name="forceRefresh">Tells the method to acquire a new token regardless of expiration.</param>
        /// <param name="agentIdentity">The identity of the agent on whose behalf the token is requested.</param>
        /// <param name="agentUser">The user on whose behalf the token is requested.</param>
        /// <param name="tenantId">The tenant ID for which the token is requested.</param>
        /// <returns>A <see cref="Task{AuthenticationResult}"/> object.</returns>
        public Task<AuthenticatorResult> GetTokenAsync(bool forceRefresh = false, string agentIdentity = "", string agentUser = "", string tenantId = "");
    }
}
