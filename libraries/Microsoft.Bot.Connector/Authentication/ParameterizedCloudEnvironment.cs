// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    internal class ParameterizedCloudEnvironment : ICloudEnvironment
    {
        private string _channelService;
        private string _toChannelFromBotLoginUrl;
        private string _toChannelFromBotOAuthScope;
        private string _toBotFromChannelTokenIssuer;
        private string _oAuthUrl;
        private string _toBotFromChannelOpenIdMetadataUrl;
        private string _toBotFromEmulatorOpenIdMetadataUrl;
        private string _callerId;

        public ParameterizedCloudEnvironment(
            string channelService,
            string toChannelFromBotLoginUrl,
            string toChannelFromBotOAuthScope,
            string toBotFromChannelTokenIssuer,
            string oAuthUrl,
            string toBotFromChannelOpenIdMetadataUrl,
            string toBotFromEmulatorOpenIdMetadataUrl,
            string callerId)
        {
            _channelService = channelService;
            _toChannelFromBotLoginUrl = toChannelFromBotLoginUrl;
            _toChannelFromBotOAuthScope = toChannelFromBotOAuthScope;
            _toBotFromChannelTokenIssuer = toBotFromChannelTokenIssuer;
            _oAuthUrl = oAuthUrl;
            _toBotFromChannelOpenIdMetadataUrl = toBotFromChannelOpenIdMetadataUrl;
            _toBotFromEmulatorOpenIdMetadataUrl = toBotFromEmulatorOpenIdMetadataUrl;
            _callerId = callerId;
        }

        public Task<(ClaimsIdentity claimsIdentity, ServiceClientCredentials credentials, string scope, string callerId)> AuthenticateRequestAsync(Activity activity, string authHeader, ICredentialProvider credentialProvider, HttpClient httpClient, ILogger logger)
        {
            throw new NotImplementedException();
        }
    }
}
