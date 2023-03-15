// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Microsoft.Azure.Services.AppAuthentication;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <inheritdoc />
    [Obsolete("This class is deprecated.", false)]
    public class JwtTokenProviderFactory : IJwtTokenProviderFactory
    {
        /// <inheritdoc />
        public AzureServiceTokenProvider CreateAzureServiceTokenProvider(string appId, HttpClient customHttpClient = null)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            // https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=dotnet
            // "RunAs=App;AppId=<client-id-guid>" for user-assigned managed identities
            var connectionString = $"RunAs=App;AppId={appId}";
            return customHttpClient == null
                ? new AzureServiceTokenProvider(connectionString)
                : new AzureServiceTokenProvider(connectionString, httpClientFactory: new ConstantHttpClientFactory(customHttpClient));
        }
    }
}
