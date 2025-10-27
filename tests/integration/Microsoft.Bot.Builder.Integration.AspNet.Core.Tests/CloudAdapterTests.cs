// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Abstractions;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    public class CloudAdapterTests
    {
        [Fact]
        public async Task BasicMessageActivity()
        {
            // Arrange
            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns(HttpMethods.Post);
            httpRequestMock.Setup(r => r.Body).Returns(CreateMessageActivityStream());
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);

            var mockTokenProvider = new Mock<IAuthorizationHeaderProvider>();

            var httpResponseMock = new Mock<HttpResponse>();

            var botMock = new Mock<IBot>();
            botMock.Setup(b => b.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var adapter = new CloudAdapter(mockTokenProvider.Object);
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, botMock.Object);

            // Assert
            botMock.Verify(m => m.OnTurnAsync(It.Is<TurnContext>(tc => true), It.Is<CancellationToken>(ct => true)), Times.Once());
        }

        [Fact]
        public async Task InvokeActivity()
        {
            // Arrange
            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns(HttpMethods.Post);
            httpRequestMock.Setup(r => r.Body).Returns(CreateInvokeActivityStream());
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);

            var response = new MemoryStream();
            var httpResponseMock = new Mock<HttpResponse>();
            httpResponseMock.Setup(r => r.Body).Returns(response);

            var mockTokenProvider = new Mock<IAuthorizationHeaderProvider>();

            var bot = new InvokeResponseBot();

            // Act
            var adapter = new CloudAdapter(mockTokenProvider.Object);
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, bot);

            // Assert
            using (var stream = new MemoryStream(response.GetBuffer()))
            using (var reader = new StreamReader(stream))
            {
                var s = reader.ReadToEnd();
                var json = JObject.Parse(s);
                Assert.Equal("im.feeling.really.attacked.right.now", json["quite.honestly"]);
            }
        }

        [Theory]
        [InlineData("DELETE")]
        [InlineData("PUT")]
        [InlineData("GET")]
        [InlineData("HEAD")]
        [InlineData("PATCH")]
        public async Task MethodNotAllowed(string httpMethod)
        {
            // Arrange
            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var webSocketManagerMock = new Mock<WebSocketManager>();
            webSocketManagerMock.Setup(w => w.IsWebSocketRequest).Returns(false);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.WebSockets).Returns(webSocketManagerMock.Object);
            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns(httpMethod);
            httpRequestMock.Setup(r => r.Body).Returns(CreateMessageActivityStream());
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);
            httpRequestMock.Setup(r => r.HttpContext).Returns(httpContextMock.Object);

            var httpResponseMock = new Mock<HttpResponse>().SetupAllProperties();

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(CreateInternalHttpResponse()));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(cf => cf.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var bot = new MessageBot();

            // Act
            var mockTokenProvider = new Mock<IAuthorizationHeaderProvider>();
            var cloudEnvironment = BotFrameworkAuthenticationFactory.Create(null, null, false, null, null, null, null, null, null, null, new PasswordServiceClientCredentialFactory(), new AuthenticationConfiguration(), httpClientFactoryMock.Object, null);
            var adapter = new CloudAdapter(mockTokenProvider.Object, cloudEnvironment);
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, bot);

            // Assert
            Assert.Equal((int)HttpStatusCode.MethodNotAllowed, httpResponseMock.Object.StatusCode);
        }

        [Fact]
        public void ConstructorWithConfiguration()
        {
            // Arrange
            var appSettings = new Dictionary<string, string>
            {
                { "MicrosoftAppId", "appId" },
                { "MicrosoftAppPassword", "appPassword" },
                { "ChannelService", GovernmentAuthenticationConstants.ChannelService }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(appSettings)
                .Build();

            var mockTokenProvider = new Mock<IAuthorizationHeaderProvider>();

            // Act
            _ = new CloudAdapter(mockTokenProvider.Object, configuration);

            // Assert

            // TODO: work out what might be a reasonable side effect
        }

        [Fact]
        public async Task BadRequest()
        {
            // Arrange
            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns(HttpMethods.Post);
            httpRequestMock.Setup(r => r.Body).Returns(CreateBadRequestStream());
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);

            var httpResponseMock = new Mock<HttpResponse>();
            httpResponseMock.SetupProperty(x => x.StatusCode);

            var mockTokenProvider = new Mock<IAuthorizationHeaderProvider>();

            var botMock = new Mock<IBot>();
            botMock.Setup(b => b.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var adapter = new CloudAdapter(mockTokenProvider.Object);
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, botMock.Object);

            // Assert
            botMock.Verify(m => m.OnTurnAsync(It.Is<TurnContext>(tc => true), It.Is<CancellationToken>(ct => true)), Times.Never());
            Assert.Equal((int)HttpStatusCode.BadRequest, httpResponseMock.Object.StatusCode);
        }

        [Fact]
        public async Task InjectCloudEnvironment()
        {
            // Arrange
            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns(HttpMethods.Post);
            httpRequestMock.Setup(r => r.Body).Returns(CreateMessageActivityStream());
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);

            var httpResponseMock = new Mock<HttpResponse>();

            var botMock = new Mock<IBot>();
            botMock.Setup(b => b.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var authenticateRequestResult = new AuthenticateRequestResult
            {
                ClaimsIdentity = new ClaimsIdentity(),
                ConnectorFactory = new TestConnectorFactory(),
                Audience = "audience",
                CallerId = "callerId"
            };

            var userTokenClient = new TestUserTokenClient("appId");

            var cloudEnvironmentMock = new Mock<BotFrameworkAuthentication>();
            cloudEnvironmentMock.Setup(ce => ce.AuthenticateRequestAsync(It.IsAny<Activity>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(authenticateRequestResult));
            cloudEnvironmentMock.Setup(ce => ce.CreateUserTokenClientAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<UserTokenClient>(userTokenClient));

            var httpClient = new Mock<HttpClient>();

            var mockTokenProvider = new Mock<IAuthorizationHeaderProvider>();

            // Act
            var adapter = new CloudAdapter(mockTokenProvider.Object, cloudEnvironmentMock.Object);
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, botMock.Object);

            // Assert
            botMock.Verify(m => m.OnTurnAsync(It.Is<TurnContext>(tc => true), It.Is<CancellationToken>(ct => true)), Times.Once());
            cloudEnvironmentMock.Verify(ce => ce.AuthenticateRequestAsync(It.Is<Activity>(tc => true), It.Is<string>(tc => true), It.Is<CancellationToken>(ct => true)), Times.Once());
        }

        [Fact]
        public async Task CloudAdapterConnectorFactory()
        {
            // this is just a basic test to verify the wire-up of a ConnectorFactory in the CloudAdapter

            // Arrange

            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Method).Returns(HttpMethods.Post);
            httpRequestMock.Setup(r => r.Body).Returns(CreateMessageActivityStream());
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);

            var httpResponseMock = new Mock<HttpResponse>();

            var claimsIdentity = new ClaimsIdentity();

            var authenticateRequestResult = new AuthenticateRequestResult
            {
                ClaimsIdentity = claimsIdentity,
                ConnectorFactory = new TestConnectorFactory(),
                Audience = "audience",
                CallerId = "callerId"
            };

            var userTokenClient = new TestUserTokenClient("appId");

            var cloudEnvironmentMock = new Mock<BotFrameworkAuthentication>();
            cloudEnvironmentMock.Setup(ce => ce.AuthenticateRequestAsync(It.IsAny<Activity>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(authenticateRequestResult));
            cloudEnvironmentMock.Setup(ce => ce.CreateConnectorFactory(It.IsAny<ClaimsIdentity>())).Returns(new TestConnectorFactory());
            cloudEnvironmentMock.Setup(ce => ce.CreateUserTokenClientAsync(It.IsAny<ClaimsIdentity>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<UserTokenClient>(userTokenClient));

            var bot = new ConnectorFactoryBot();

            var mockTokenProvider = new Mock<IAuthorizationHeaderProvider>();

            // Act
            var adapter = new CloudAdapter(mockTokenProvider.Object, cloudEnvironmentMock.Object);
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, bot);

            // Assert
            //Assert.Equal("audience", bot.Authorization.Parameter);
            //Assert.Equal(claimsIdentity, bot.Identity);
            //Assert.Equal(userTokenClient, bot.UserTokenClient);
            //Assert.True(bot.ConnectorClient != null);
            //Assert.True(bot.BotCallbackHandler != null);
        }

        private Stream CreateMessageActivityStream()
        {
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "Hello"
            };
            return CreateStream(activity);
        }

        private Stream CreateInvokeActivityStream()
        {
            var activity = new Activity
            {
                Type = ActivityTypes.Invoke,
                Name = "test",
                Value = new { foo = "bar" }
            };
            return CreateStream(activity);
        }

        private Stream CreateBadRequestStream()
        {
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = null // Intentionally set to null to cause a bad request
            };
            return CreateStream(activity);
        }

        private Stream CreateStream(Activity activity)
        {
            var json = JsonConvert.SerializeObject(activity);
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, new UTF8Encoding(false));
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private HttpResponseMessage CreateInternalHttpResponse()
        {
            var response = new HttpResponseMessage((HttpStatusCode)429);
            response.Content = new StringContent(JsonConvert.SerializeObject(new { quite = new { honestly = "im.feeling.really.attacked.right.now" } }), Encoding.UTF8, "application/json");
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromSeconds(10),
                MustRevalidate = true
            };
            return response;
        }
    }
}
