﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Microsoft.Identity.Client;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// HttpClientFactory that always returns the same HttpClient instance for AcquireTokenAsync calls.
    /// </summary>
    internal class ConstantHttpClientFactory : IMsalHttpClientFactory
    {
        private readonly HttpClient httpClient;

        public ConstantHttpClientFactory(HttpClient client)
        {
            httpClient = client ?? throw new ArgumentNullException(nameof(client));
        }

        public HttpClient GetHttpClient()
        {
            return httpClient;
        }
    }
}
