// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class TokenTests
    {
        [Fact]
        public void TokenExchangeInvokeRequestInits()
        {
            var id = "id";
            var connectionName = "connectionName";
            var token = "token";
            var properties = new JObject();

            var tokenExchangeInvokeRequest = new TokenExchangeInvokeRequest()
            {
                Id = id,
                ConnectionName = connectionName,
                Token = token,
                Properties = properties
            };

            Assert.NotNull(tokenExchangeInvokeRequest);
            Assert.IsType<TokenExchangeInvokeRequest>(tokenExchangeInvokeRequest);
            Assert.Equal(id, tokenExchangeInvokeRequest.Id);
            Assert.Equal(connectionName, tokenExchangeInvokeRequest.ConnectionName);
            Assert.Equal(token, tokenExchangeInvokeRequest.Token);
            Assert.Equal(properties, tokenExchangeInvokeRequest.Properties);
        }
        
        [Fact]
        public void TokenExchangeInvokeResponseInits()
        {
            var id = "id";
            var connectionName = "connectionName";
            var failureDetail = "failureDetail";
            var properties = new JObject();

            var tokenExchangeInvokeResponse = new TokenExchangeInvokeResponse()
            {
                Id = id,
                ConnectionName = connectionName,
                FailureDetail = failureDetail,
                Properties = properties,
            };

            Assert.NotNull(tokenExchangeInvokeResponse);
            Assert.IsType<TokenExchangeInvokeResponse>(tokenExchangeInvokeResponse);
            Assert.Equal(id, tokenExchangeInvokeResponse.Id);
            Assert.Equal(connectionName, tokenExchangeInvokeResponse.ConnectionName);
            Assert.Equal(failureDetail, tokenExchangeInvokeResponse.FailureDetail);
            Assert.Equal(properties, tokenExchangeInvokeResponse.Properties);
        }

        [Fact]
        public void TokenExchangeInvokeResponseInitsWithNoArgs()
        {
            var tokenExchangeInvokeResponse = new TokenExchangeInvokeResponse();

            Assert.NotNull(tokenExchangeInvokeResponse);
            Assert.IsType<TokenExchangeInvokeResponse>(tokenExchangeInvokeResponse);
        }

        [Fact]
        public void TokenExchangeRequestInits()
        {
            var uri = "http://example.com";
            var token = "token";

            var tokenExchangeRequest = new TokenExchangeRequest(uri, token);

            Assert.NotNull(tokenExchangeRequest);
            Assert.IsType<TokenExchangeRequest>(tokenExchangeRequest);
            Assert.Equal(uri, tokenExchangeRequest.Uri);
            Assert.Equal(token, tokenExchangeRequest.Token);
        }
        
        [Fact]
        public void TokenExchangeRequestInitsWithNoArgs()
        {
            var tokenExchangeRequest = new TokenExchangeRequest();

            Assert.NotNull(tokenExchangeRequest);
            Assert.IsType<TokenExchangeRequest>(tokenExchangeRequest);
        }

        [Fact]
        public void TokenExchangeResourceInits()
        {
            var id = "id";
            var uri = "http://example.com";
            var providerId = "providerId";

            var tokenExchangeResource = new TokenExchangeResource(id, uri, providerId);

            Assert.NotNull(tokenExchangeResource);
            Assert.IsType<TokenExchangeResource>(tokenExchangeResource);
            Assert.Equal(id, tokenExchangeResource.Id);
            Assert.Equal(uri, tokenExchangeResource.Uri);
            Assert.Equal(providerId, tokenExchangeResource.ProviderId);
        }
        
        [Fact]
        public void TokenExchangeResourceInitsWithNoArgs()
        {
            var tokenExchangeResource = new TokenExchangeResource();

            Assert.NotNull(tokenExchangeResource);
            Assert.IsType<TokenExchangeResource>(tokenExchangeResource);
        }

        [Fact]
        public void TokenExchangeStateInits()
        {
            var connectionName = "connectionName";
            var convo = new ConversationReference(
                "ActivityId",
                new ChannelAccount("userId"),
                new ChannelAccount("bodId"),
                new ConversationAccount(),
                "channelId",
                "serviceUrl");
            var relatesTo = new ConversationReference();
            var botUrl = "http://localhost:3978";
            var msAppId = "msAppId";

            var tokenExchangeState = new TokenExchangeState()
            {
                ConnectionName = connectionName,
                Conversation = convo,
                RelatesTo = relatesTo,
                BotUrl = botUrl,
                MsAppId = msAppId,
            };

            Assert.NotNull(tokenExchangeState);
            Assert.IsType<TokenExchangeState>(tokenExchangeState);
            Assert.Equal(connectionName, tokenExchangeState.ConnectionName);
            Assert.Equal(convo, tokenExchangeState.Conversation);
            Assert.Equal(relatesTo, tokenExchangeState.RelatesTo);
            Assert.Equal(botUrl, tokenExchangeState.BotUrl);
            Assert.Equal(msAppId, tokenExchangeState.MsAppId);
        }
        
        [Fact]
        public void TokenExchangeStateInitsWithNoArgs()
        {
            var tokenExchangeState = new TokenExchangeState();

            Assert.NotNull(tokenExchangeState);
            Assert.IsType<TokenExchangeState>(tokenExchangeState);
        }

        [Fact]
        public void TokenPollingSettingsInits()
        {
            var timeout = 1000;
            var interval = 600000;

            var tokenPollingSettings = new TokenPollingSettings()
            {
                Timeout = timeout,
                Interval = interval,
            };

            Assert.NotNull(tokenPollingSettings);
            Assert.IsType<TokenPollingSettings>(tokenPollingSettings);
            Assert.Equal(timeout, tokenPollingSettings.Timeout);
            Assert.Equal(interval, tokenPollingSettings.Interval);
        }

        [Fact]
        public void TokenRequestInits()
        {
            var provider = "provider";
            var settings = new Dictionary<string, object>() { { "key", "value" } };

            var tokenRequest = new TokenRequest(provider, settings);

            Assert.NotNull(tokenRequest);
            Assert.IsType<TokenRequest>(tokenRequest);
            Assert.Equal(provider, tokenRequest.Provider);
            Assert.Equal(settings, tokenRequest.Settings);
        }
        
        [Fact]
        public void TokenRequestInitsWithNoArgs()
        {
            var tokenRequest = new TokenRequest();

            Assert.NotNull(tokenRequest);
            Assert.IsType<TokenRequest>(tokenRequest);
        }

        [Fact]
        public void TokenResponseInits()
        {
            var channelId = "channelId";
            var connectionName = "connectionName";
            var token = "token";
            var expiration = "expiration";
            var properties = new JObject();

            var tokenResponse = new TokenResponse(channelId, connectionName, token, expiration)
            {
                Properties = properties
            };

            Assert.NotNull(tokenResponse);
            Assert.IsType<TokenResponse>(tokenResponse);
            Assert.Equal(channelId, tokenResponse.ChannelId);
            Assert.Equal(connectionName, tokenResponse.ConnectionName);
            Assert.Equal(token, tokenResponse.Token);
            Assert.Equal(expiration, tokenResponse.Expiration);
            Assert.Equal(properties, tokenResponse.Properties);
        }
        
        [Fact]
        public void TokenResponseInitsWithNoArgs()
        {
            var tokenResponse = new TokenResponse();

            Assert.NotNull(tokenResponse);
            Assert.IsType<TokenResponse>(tokenResponse);
        }

        [Fact]
        public void TokenStatusInits()
        {
            var channelId = "channelId";
            var connectionName = "connectionName";
            var hasToken = true;
            var serviceProviderDisplayName = "serviceProviderDisplayName";

            var tokenStatus = new TokenStatus(channelId, connectionName, hasToken, serviceProviderDisplayName);

            Assert.NotNull(tokenStatus);
            Assert.IsType<TokenStatus>(tokenStatus);
            Assert.Equal(channelId, tokenStatus.ChannelId);
            Assert.Equal(connectionName, tokenStatus.ConnectionName);
            Assert.Equal(hasToken, tokenStatus.HasToken);
            Assert.Equal(serviceProviderDisplayName, tokenStatus.ServiceProviderDisplayName);
        }
        
        [Fact]
        public void TokenStatusInitsWithNoArgs()
        {
            var tokenStatus = new TokenStatus();

            Assert.NotNull(tokenStatus);
            Assert.IsType<TokenStatus>(tokenStatus);
        }
    }
}
