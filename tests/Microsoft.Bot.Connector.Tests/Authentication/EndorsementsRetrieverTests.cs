// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.IdentityModel.Protocols;
using Moq;
using RichardSzalay.MockHttp;
using Xunit;

namespace Microsoft.Bot.Connector.Tests.Authentication
{
    public class EndorsementsRetrieverTests
    {
        public class ConstructorTests
        {
            [Fact]
            public void NullHttpClientShouldThrow()
            {
                Action action = () => new EndorsementsRetriever(null);

                action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("httpClient");
            }

            [Fact]
            public void SucceedsWithValidParameters()
            {
                new EndorsementsRetriever(new HttpClient());
            }
        }

        public class GetConfigurationAsyncTests
        {
            private const string FakeDocumentAddress = "http://fakeaddress";

            private readonly Mock<IDocumentRetriever> _mockDocumentRetriever;
            private readonly MockHttpMessageHandler _mockHttpMessageHandler;
            private readonly EndorsementsRetriever _endorsementsRetriever;

            public GetConfigurationAsyncTests()
            {
                _mockDocumentRetriever = new Mock<IDocumentRetriever>();
                _mockHttpMessageHandler = new MockHttpMessageHandler();
                _endorsementsRetriever = new EndorsementsRetriever(_mockHttpMessageHandler.ToHttpClient());
            }

            [Fact]
            public async Task NullAddressParameterShouldThrow()
            {
                Func<Task> action = async () => await _endorsementsRetriever.GetConfigurationAsync(null, _mockDocumentRetriever.Object, CancellationToken.None);

                (await action.Should().ThrowAsync<ArgumentNullException>()).And.ParamName.Should().Be("address");
            }

            [Fact]
            public async Task NullDocumentRetrieverParameterShouldThrow()
            {
                Func<Task> action = async () => await _endorsementsRetriever.GetConfigurationAsync(FakeDocumentAddress, null, CancellationToken.None);

                (await action.Should().ThrowAsync<ArgumentNullException>()).And.ParamName.Should().Be("retriever");
            }

            [Fact]
            public async Task ReturnsEmptyValues_EmptyObject()
            {
                _mockDocumentRetriever.Setup(dr => dr.GetDocumentAsync(FakeDocumentAddress, It.IsAny<CancellationToken>()))
                    .ReturnsAsync("{ }");

                var results = await _endorsementsRetriever.GetConfigurationAsync(FakeDocumentAddress, _mockDocumentRetriever.Object, CancellationToken.None);

                results.Should().NotBeNull().And.BeEmpty();

                _mockDocumentRetriever.Verify(dr => dr.GetDocumentAsync(FakeDocumentAddress, It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public async Task ReturnsEmptyValues_EmptyKeysArray()
            {
                _mockDocumentRetriever.Setup(dr => dr.GetDocumentAsync(FakeDocumentAddress, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(@"{ ""keys"": [ ] }");

                var results = await _endorsementsRetriever.GetConfigurationAsync(FakeDocumentAddress, _mockDocumentRetriever.Object, CancellationToken.None);

                results.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public async Task ReturnsEmptyValues_NullKeysArray()
            {
                _mockDocumentRetriever.Setup(dr => dr.GetDocumentAsync(FakeDocumentAddress, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(@"{ ""keys"": null }");

                var results = await _endorsementsRetriever.GetConfigurationAsync(FakeDocumentAddress, _mockDocumentRetriever.Object, CancellationToken.None);

                results.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public async Task ReturnsEmptyValues_InvalidKeyObjectInKeysArray()
            {
                _mockDocumentRetriever.Setup(dr => dr.GetDocumentAsync(FakeDocumentAddress, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(@"{ ""keys"": [ { } ] }");

                var results = await _endorsementsRetriever.GetConfigurationAsync(FakeDocumentAddress, _mockDocumentRetriever.Object, CancellationToken.None);

                results.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public async Task ReturnsEmptyValues_SingleKeyWithNoEndorsements()
            {
                _mockDocumentRetriever.Setup(dr => dr.GetDocumentAsync(FakeDocumentAddress, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(@"{ ""keys"": [ { ""kid"": ""keyid123"" } ] }");

                var results = await _endorsementsRetriever.GetConfigurationAsync(FakeDocumentAddress, _mockDocumentRetriever.Object, CancellationToken.None);

                results.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public async Task ReturnsEmptyValues_MultipleKeysWithNoEndorsements()
            {
                _mockDocumentRetriever.Setup(dr => dr.GetDocumentAsync(FakeDocumentAddress, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(@"{ ""keys"": [ { ""kid"": ""keyid123"" }, { ""kid"": ""keyid456"" }, { ""kid"": ""keyid789"" } ] }");

                var results = await _endorsementsRetriever.GetConfigurationAsync(FakeDocumentAddress, _mockDocumentRetriever.Object, CancellationToken.None);

                results.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public async Task ThrowsIfDocumentRetrieverThrows()
            {
                _mockDocumentRetriever.Setup(dr => dr.GetDocumentAsync(FakeDocumentAddress, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception(nameof(ThrowsIfDocumentRetrieverThrows)));

                Func<Task> action = async () => await _endorsementsRetriever.GetConfigurationAsync(FakeDocumentAddress, _mockDocumentRetriever.Object, CancellationToken.None);

                (await action.Should().ThrowAsync<Exception>()).And.Message.Should().Be(nameof(ThrowsIfDocumentRetrieverThrows));
            }
        }

        public class GetDocumentAsyncTests
        {
            private const string FakeDocumentAddress = "http://fakeendorsementsaddress";
            private const string FakeKeysAddressUrl = "http://fakekeysaddress";

            private readonly MockHttpMessageHandler _mockHttpMessageHandler;
            private readonly EndorsementsRetriever _endorsementsRetriever;

            public GetDocumentAsyncTests()
            {
                _mockHttpMessageHandler = new MockHttpMessageHandler();
                _endorsementsRetriever = new EndorsementsRetriever(_mockHttpMessageHandler.ToHttpClient());
            }

            [Fact]
            public async Task NullAddressParameterShouldThrow()
            {
                Func<Task> action = async () => await _endorsementsRetriever.GetDocumentAsync(null, CancellationToken.None);

                (await action.Should().ThrowAsync<ArgumentNullException>()).And.ParamName.Should().Be("address");
            }

            [Fact]
            public async Task NonSuccessHttpStatusResponseForEndorsementsDocumentShouldThrow()
            {
                _mockHttpMessageHandler.When(FakeDocumentAddress)
                    .Respond(HttpStatusCode.NotFound);

                Func<Task> action = async () => await _endorsementsRetriever.GetDocumentAsync(FakeDocumentAddress, CancellationToken.None);

                await action.Should().ThrowAsync<Exception>();
            }

            [Fact]
            public async Task NonSuccessHttpStatusResponseForWebKeySetDocumentShouldThrow()
            {
                _mockHttpMessageHandler.When(FakeDocumentAddress)
                    .Respond(new StringContent($@"{{ ""{EndorsementsRetriever.JsonWebKeySetUri}"": ""{FakeKeysAddressUrl}"" }}"));

                _mockHttpMessageHandler.When(FakeKeysAddressUrl)
                    .Respond(HttpStatusCode.NotFound);

                Func<Task> action = async () => await _endorsementsRetriever.GetDocumentAsync(FakeDocumentAddress, CancellationToken.None);

                await action.Should().ThrowAsync<Exception>();
            }

            [Fact]
            public async Task EmptyEndorsementsResponseReturnsEmptyResult()
            {
                _mockHttpMessageHandler.When(FakeDocumentAddress)
                    .Respond(new StringContent(string.Empty));

                var result = await _endorsementsRetriever.GetDocumentAsync(FakeDocumentAddress, CancellationToken.None);

                result.Should().BeEmpty();
            }

            [Fact]
            public async Task EmptyEndorsementsDocumentReturnsEmptyResult()
            {
                _mockHttpMessageHandler.When(FakeDocumentAddress)
                    .Respond(new StringContent("{}"));

                var result = await _endorsementsRetriever.GetDocumentAsync(FakeDocumentAddress, CancellationToken.None);

                result.Should().BeEmpty();

                _mockHttpMessageHandler.VerifyNoOutstandingRequest();
            }

            [Fact]
            public async Task EndorsementsDocumentWithNoKeyReturnsEmptyResult()
            {
                _mockHttpMessageHandler.When(FakeDocumentAddress)
                    .Respond(new StringContent(@"{ ""somkey1"": 123, ""somekey2"": ""hello world"" }"));

                var result = await _endorsementsRetriever.GetDocumentAsync(FakeDocumentAddress, CancellationToken.None);

                result.Should().BeEmpty();
            }

            [Fact]
            public async Task EmptyKeySetDocumentResponseReturnsEmptyResult()
            {
                _mockHttpMessageHandler.When(FakeDocumentAddress)
                    .Respond(new StringContent($@"{{ ""{EndorsementsRetriever.JsonWebKeySetUri}"": ""{FakeKeysAddressUrl}"" }}"));

                _mockHttpMessageHandler.When(FakeKeysAddressUrl)
                    .Respond(new StringContent(string.Empty));

                var result = await _endorsementsRetriever.GetDocumentAsync(FakeDocumentAddress, CancellationToken.None);

                result.Should().BeEmpty();
            }

            [Fact]
            public async Task ExpectedKeySetDocumentReturnsSuccessfully()
            {
                _mockHttpMessageHandler.When(FakeDocumentAddress)
                    .Respond(new StringContent($@"{{ ""{EndorsementsRetriever.JsonWebKeySetUri}"": ""{FakeKeysAddressUrl}"" }}"));

                _mockHttpMessageHandler.When(FakeKeysAddressUrl)
                    .Respond(new StringContent(nameof(ExpectedKeySetDocumentReturnsSuccessfully)));

                var result = await _endorsementsRetriever.GetDocumentAsync(FakeDocumentAddress, CancellationToken.None);

                result.Should().Be(nameof(ExpectedKeySetDocumentReturnsSuccessfully));
            }
        }
    }
}
