// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Microsoft.Azure.Services.AppAuthentication;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// A factory that can create OAuth token providers for generating JWT auth tokens.
    /// </summary>
    [Obsolete("This class is deprecated.", false)]
    public interface IJwtTokenProviderFactory
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AzureServiceTokenProvider"/> class.
        /// </summary>
        /// <param name="appId">Client id for the managed identity to be used for acquiring tokens.</param>
        /// <param name="customHttpClient">A customized instance of the HttpClient class.</param>
        /// <returns>A new instance of the <see cref="AzureServiceTokenProvider"/> class.</returns>
        AzureServiceTokenProvider CreateAzureServiceTokenProvider(string appId, HttpClient customHttpClient = null);
    }
}
