// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// IServiceClientCredentialsFactory interface. This interface allows Bots to provide their own
    /// credentials for bot to channel or skill bot to parent bot calls.
    /// </summary>
    public interface IServiceClientCredentialsFactory
    {
        /// <summary>
        /// A factiry method for creating ServiceClientCredentials.
        /// </summary>
        /// <param name="appId">The appId.</param>
        /// <param name="oauthScope">The oauth scope.</param>
        /// <param name="loginEndpoint">The login url.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ServiceClientCredentials> CreateCredentialsAsync(string appId, string oauthScope, string loginEndpoint);
    }
}
