// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class TokenExchangeTests
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
    }
}
