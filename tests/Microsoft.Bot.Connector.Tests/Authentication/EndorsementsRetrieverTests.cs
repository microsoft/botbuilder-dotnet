using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using RichardSzalay.MockHttp;
using System.Net.Http;
using Microsoft.IdentityModel.Protocols;
using System.Threading;

namespace Microsoft.Bot.Connector.Authentication.Tests
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
            public void NullAddressParameterShouldThrow()
            {
                Func<Task> action = async () => await _endorsementsRetriever.GetConfigurationAsync(null, _mockDocumentRetriever.Object, CancellationToken.None);

                action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("address");
            }

            [Fact]
            public void NullDocumentRetrieverParameterShouldThrow()
            {
                Func<Task> action = async () => await _endorsementsRetriever.GetConfigurationAsync(FakeDocumentAddress, null, CancellationToken.None);

                action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("retriever");
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
                    .ReturnsAsync(@"{ ""keys"": [ { ""keyid"": ""keyid123"" } ] }");

                var results = await _endorsementsRetriever.GetConfigurationAsync(FakeDocumentAddress, _mockDocumentRetriever.Object, CancellationToken.None);

                results.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public async Task ReturnsEmptyValues_MultipleKeysWithNoEndorsements()
            {
                _mockDocumentRetriever.Setup(dr => dr.GetDocumentAsync(FakeDocumentAddress, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(@"{ ""keys"": [ { ""keyid"": ""keyid123"" }, { ""keyid"": ""keyid456"" }, { ""keyid"": ""keyid789"" } ] }");

                var results = await _endorsementsRetriever.GetConfigurationAsync(FakeDocumentAddress, _mockDocumentRetriever.Object, CancellationToken.None);

                results.Should().NotBeNull().And.BeEmpty();
            }

            [Fact]
            public void ThrowsIfDocumentRetrieverThrows()
            {
                _mockDocumentRetriever.Setup(dr => dr.GetDocumentAsync(FakeDocumentAddress, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception(nameof(ThrowsIfDocumentRetrieverThrows)));

                Func<Task> action = async () => await _endorsementsRetriever.GetConfigurationAsync(FakeDocumentAddress, _mockDocumentRetriever.Object, CancellationToken.None);

                action.Should().Throw<Exception>().And.Message.Should().Be(nameof(ThrowsIfDocumentRetrieverThrows));
            }
        }
    }
}
