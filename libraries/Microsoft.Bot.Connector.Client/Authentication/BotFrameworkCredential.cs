// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;

namespace Microsoft.Bot.Connector.Client.Authentication
{
    /// <summary>
    /// The <see cref="BotFrameworkCredential"/> class that allows Bots to provide their own
    /// <see cref="TokenCredential"/> for bot to channel or skill bot to parent bot calls.
    /// </summary>
    public class BotFrameworkCredential
    {
        private readonly TokenCredential _credential;
        private readonly string _appId;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotFrameworkCredential"/> class with the provided credential.
        /// </summary>
        /// <param name="credential">The <see cref="TokenCredential"/> to use for authentication.</param>
        /// <param name="appId">The app ID.</param>
        public BotFrameworkCredential(TokenCredential credential, string appId = "")
        {
            _credential = credential ?? throw new ArgumentNullException(nameof(credential));
            _appId = appId;
        }

        /// <summary>
        /// Validates an app ID.
        /// </summary>
        /// <param name="appId">The app ID to validate.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result is true if <paramref name="appId"/>
        /// is valid for the controller; otherwise, false.
        /// <para>
        /// This method is async to enable custom implementations
        /// that may need to call out to serviced to validate the appId / password pair.
        /// </para></remarks>
        public virtual Task<bool> IsValidAppIdAsync(string appId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(appId == _appId);
        }

        /// <summary>
        /// Checks whether bot authentication is disabled.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful and bot authentication is disabled, the result
        /// is true; otherwise, false.
        /// <para>
        /// This method is async to enable custom implementations
        /// that may need to call out to serviced to validate the appId / password pair.
        /// </para></remarks>
        public virtual Task<bool> IsAuthenticationDisabledAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(string.IsNullOrWhiteSpace(_appId));
        }

        /// <summary>
        /// Gets the <see cref="TokenCredential"/> to use for creating OAuth tokens.
        /// </summary>
        /// <returns>The <see cref="TokenCredential"/>.</returns>
        public TokenCredential GetTokenCredential()
        {
            return _credential;
        }
    }
}
