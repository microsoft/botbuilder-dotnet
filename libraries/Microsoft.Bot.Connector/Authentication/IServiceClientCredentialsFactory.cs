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
        /// Validates an app ID.
        /// </summary>
        /// <param name="appId">The app ID to validate.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result is true if <paramref name="appId"/>
        /// is valid for the controller; otherwise, false.
        /// <para>
        /// This method is async to enable custom implementations
        /// that may need to call out to serviced to validate the appId / password pair.
        /// </para></remarks>
        Task<bool> IsValidAppIdAsync(string appId);

        /// <summary>
        /// Checks whether bot authentication is disabled.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful and bot authentication is disabled, the result
        /// is true; otherwise, false.
        /// <para>
        /// This method is async to enable custom implementations
        /// that may need to call out to serviced to validate the appId / password pair.
        /// </para></remarks>
        Task<bool> IsAuthenticationDisabledAsync();

        /// <summary>
        /// A factory method for creating ServiceClientCredentials.
        /// </summary>
        /// <param name="appId">The appId.</param>
        /// <param name="oauthScope">The oauth scope.</param>
        /// <param name="loginEndpoint">The login url.</param>
        /// <param name="validateAuthority">The validate authority vale to use.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ServiceClientCredentials> CreateCredentialsAsync(string appId, string oauthScope, string loginEndpoint, bool validateAuthority);
    }
}
