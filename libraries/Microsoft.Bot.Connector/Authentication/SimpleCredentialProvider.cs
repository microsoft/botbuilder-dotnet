// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// A simple implementation of the <see cref="ICredentialProvider"/> interface.
    /// </summary>
    public class SimpleCredentialProvider : ICredentialProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCredentialProvider"/> class.
        /// with empty credentials.
        /// </summary>
        public SimpleCredentialProvider()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCredentialProvider"/> class.
        /// with the provided credentials.
        /// </summary>
        /// <param name="appId">The app ID.</param>
        /// <param name="password">The app password.</param>
        public SimpleCredentialProvider(string appId, string password)
        {
            this.AppId = appId;
            this.Password = password;
        }

        /// <summary>
        /// Gets or sets the app ID for this credential.
        /// </summary>
        /// <value>
        /// The app ID for this credential.
        /// </value>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the app password for this credential.
        /// </summary>
        /// <value>
        /// The app password for this credential.
        /// </value>
        public string Password { get; set; }

        /// <summary>
        /// Validates an app ID.
        /// </summary>
        /// <param name="appId">The app ID to validate.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result is true if <paramref name="appId"/>
        /// is valid for the controller; otherwise, false.</remarks>
        public Task<bool> IsValidAppIdAsync(string appId)
        {
            return Task.FromResult(appId == AppId);
        }

        /// <summary>
        /// Gets the app password for a given bot app ID.
        /// </summary>
        /// <param name="appId">The ID of the app to get the password for.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful and the app ID is valid, the result
        /// contains the password; otherwise, null.
        /// </remarks>
        public Task<string> GetAppPasswordAsync(string appId)
        {
            return Task.FromResult((appId == this.AppId) ? this.Password : null);
        }

        /// <summary>
        /// Checks whether bot authentication is disabled.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful and bot authentication is disabled, the result
        /// is true; otherwise, false.
        /// </remarks>
        public Task<bool> IsAuthenticationDisabledAsync()
        {
            return Task.FromResult(string.IsNullOrEmpty(AppId));
        }
    }
}
