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
        /// Validate AppId.
        /// </summary>
        /// <remarks>
        /// This method is async to enable custom implementations
        /// that may need to call out to serviced to validate the appId / password pair.
        /// </remarks>
        /// <param name="appId"></param>
        /// <returns>true if it is a valid AppId for the controller</returns>
        Task<bool> IsValidAppIdAsync(string appId);

        /// <summary>
        /// Get the app password for a given bot appId, if it is not a valid appId, return Null
        /// </summary>
        /// <remarks>
        /// This method is async to enable custom implementations
        /// that may need to call out to serviced to validate the appId / password pair.
        /// </remarks>
        /// <param name="appId">bot appid</param>
        /// <returns>password or null for invalid appid</returns>
        Task<string> GetAppPasswordAsync(string appId);

        /// <summary>
        /// Checks if bot authentication is disabled.
        /// </summary>
        /// <returns>true if bot authentication is disabled.</returns>
        /// <remarks>
        /// This method is async to enable custom implementations
        /// that may need to call out to serviced to validate the appId / password pair.
        /// </remarks>
        Task<bool> IsAuthenticationDisabledAsync();
    }
}
