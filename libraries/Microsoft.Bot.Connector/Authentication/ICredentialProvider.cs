// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// CredentialProvider interface. This interface allows Bots to provide their own
    /// implementation of what is, and what is not, a valid appId and password. This is
    /// useful in the case of multi-tenant bots, where the bot may need to call
    /// out to a service to determine if a particular appid/password pair
    /// is valid.
    ///
    /// For Single Tenant bots (the vast majority) the simple static providers
    /// are sufficient.
    /// </summary>
    public interface ICredentialProvider
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
        /// Gets the app password for a given bot app ID.
        /// </summary>
        /// <param name="appId">The ID of the app to get the password for.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful and the app ID is valid, the result
        /// contains the password; otherwise, null.
        /// <para>
        /// This method is async to enable custom implementations
        /// that may need to call out to serviced to validate the appId / password pair.
        /// </para></remarks>
        Task<string> GetAppPasswordAsync(string appId);

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
    }
}
