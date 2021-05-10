// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Authentication
{
    internal class ConnectorFactoryImpl : ConnectorFactory
    {
        private readonly string _appId;
        private readonly string _toChannelFromBotOAuthScope;
        private readonly string _loginEndpoint;
        private readonly bool _validateAuthority;
        private readonly ServiceClientCredentialsFactory _credentialFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public ConnectorFactoryImpl(string appId, string toChannelFromBotOAuthScope, string loginEndpoint, bool validateAuthority, ServiceClientCredentialsFactory credentialFactory, IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _appId = appId;
            _toChannelFromBotOAuthScope = toChannelFromBotOAuthScope;
            _loginEndpoint = loginEndpoint;
            _validateAuthority = validateAuthority;
            _credentialFactory = credentialFactory;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public override async Task<IConnectorClient> CreateAsync(string serviceUrl, string audience, CancellationToken cancellationToken)
        {
            // Use the credentials factory to create credentails specific to this particular cloud environment.
            var credentials = await _credentialFactory.CreateCredentialsAsync(_appId, audience ?? _toChannelFromBotOAuthScope, _loginEndpoint, _validateAuthority, cancellationToken).ConfigureAwait(false);

            // A new connector client for making calls against this serviceUrl using credentials derived from the current appId and the specified audience.
#pragma warning disable CA2000 // Dispose objects before losing scope
            var httpClient = _httpClientFactory?.CreateClient() ?? new HttpClient();
            ConnectorClient.AddDefaultRequestHeaders(httpClient);
            return new ConnectorClient(new Uri(serviceUrl), credentials, httpClient, true);
#pragma warning restore CA2000 // Dispose objects before losing scope
        }
    }
}
