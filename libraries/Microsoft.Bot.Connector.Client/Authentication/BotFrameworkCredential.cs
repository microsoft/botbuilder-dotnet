// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

namespace Microsoft.Bot.Connector.Client.Authentication
{
    internal enum MicrosoftAppType
    {
        /// <summary>
        /// MultiTenant app which uses botframework.com tenant to acquire tokens.
        /// </summary>
        MultiTenant,

        /// <summary>
        /// SingleTenant app which uses the bot's host tenant to acquire tokens.
        /// </summary>
        SingleTenant,

        /// <summary>
        /// App with a user assigned Managed Identity (MSI), which will be used as the AppId for token acquisition.
        /// </summary>
        UserAssignedMsi
    }

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
        /// <param name="appType">The Microsoft App Type of the Azure Active Directory App registration: MultiTenant, SingleTenant, UserAssignedMsi.</param>
        /// <param name="tenantId">The Azure Active Directory tenant (directory) Id of the service principal.</param>
        /// <param name="appId">The client (application) ID of the service principal.</param>
        /// <param name="appPassword">A client secret that was generated for the App Registration used to authenticate the client.</param>
        public BotFrameworkCredential(string appType = null, string tenantId = null, string appId = null, string appPassword = null)
        {
            var parsedAppType = Enum.TryParse(appType, ignoreCase: true, out MicrosoftAppType parsed)
                ? parsed
                : MicrosoftAppType.MultiTenant; // default

            switch (parsedAppType)
            {
                case MicrosoftAppType.UserAssignedMsi:
                    if (string.IsNullOrWhiteSpace(appId))
                    {
                        _credential = new EmptyTokenCredential();
                    }
                    else
                    {
                        _credential = new ManagedIdentityCredential(appId);
                    }

                    break;

                case MicrosoftAppType.SingleTenant:
                    if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appPassword))
                    {
                        _credential = new EmptyTokenCredential();
                    }
                    else
                    {
                        _credential = new ClientSecretCredential(tenantId, appId, appPassword);
                    }

                    break;
                default: // MultiTenant
                    if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appPassword))
                    {
                        _credential = new EmptyTokenCredential();
                    }
                    else
                    {
                        _credential = new ClientSecretCredential(tenantId, appId, appPassword);
                    }

                    break;
            }

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

        private class EmptyTokenCredential : TokenCredential
        {
            public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            {
                return await Task.FromResult(new AccessToken()).ConfigureAwait(false);
            }

            public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            {
                return new AccessToken();
            }
        }
    }
}
