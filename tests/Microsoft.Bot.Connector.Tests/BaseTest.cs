// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Azure.Test.HttpRecorder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Moq;

namespace Microsoft.Bot.Connector.Tests
{
    /// <summary>
    /// WORKING WITH THESE TESTS
    ///   To run mocked and without recording:
    ///       1. Set HttpRecorderMode to Playback
    ///
    ///   To run live or unmocked:
    ///       1. Set HttpRecorderMode to None
    ///       2. Set the following properties (all must be valid):
    ///           * ClientId (appId)
    ///           * ClientSecret (appPass)
    ///           * UserId (from slack)
    ///           * BotId (from slack)
    ///       3. Ensure the appId has Slack channel enabled and you've installed the bot in Slack
    ///
    ///    To re-record:
    ///      1. All from live/unmocked, except set HttpRecorderMode to Record.
    ///      2. Once done recording, copy recording sessions from ...Microsoft.Bot.Connector.Tests\bin\Debug\net8.0\SessionRecords
    ///             to ...Microsoft.Bot.Connector.Tests\SessionRecords.
    /// </summary>
    public class BaseTest
    {
        private readonly HttpRecorderMode mode = HttpRecorderMode.Playback;

        private readonly string token;

        public BaseTest()
        {
            if (mode == HttpRecorderMode.Record || mode == HttpRecorderMode.None)
            {
                var credentials = new MicrosoftAppCredentials(ClientId, ClientSecret);
                var task = credentials.GetTokenAsync();
                task.Wait();
                this.token = task.Result;

                // Helpful for generating recordings when debugging locally
                Environment.SetEnvironmentVariable("AZURE_TEST_MODE", mode.ToString());
            }
            else
            {
                this.token = "STUB_TOKEN";
            }

            Bot = new ChannelAccount() { Id = BotId };
            User = new ChannelAccount() { Id = UserId };
        }

        public ChannelAccount Bot { get; private set; }

        public ChannelAccount User { get; private set; }

        protected static Uri HostUri { get; set; } = new Uri("https://slack.botframework.com", UriKind.Absolute);

        protected string ClientId { get; private set; } = "[MSAPP_ID]";

        protected string ClientSecret { get; private set; } = "[MSAPP_PASSWORD]";

        protected string UserId { get; private set; } = "UK8CH2281:TKGSUQHQE";

        protected string BotId { get; private set; } = "BKGSYSTFG:TKGSUQHQE";

        private string ClassName => GetType().FullName;

#pragma warning disable 162

        public async Task AssertTracingFor(
            Func<Task> doTest,
            string tracedMethodName,
            bool isSuccesful = true,
            Func<HttpRequestMessage, bool> assertHttpRequestMessage = null)
        {
            tracedMethodName = tracedMethodName.EndsWith("Async") ? tracedMethodName.Remove(tracedMethodName.LastIndexOf("Async")) : tracedMethodName;

            var traceInterceptor = new Mock<IServiceClientTracingInterceptor>();
            var invocationIds = new List<string>();
            traceInterceptor.Setup(
                i => i.EnterMethod(It.IsAny<string>(), It.IsAny<object>(), tracedMethodName, It.IsAny<IDictionary<string, object>>()))
                .Callback((string id, object instance, string method, IDictionary<string, object> parameters) => invocationIds.Add(id));

            ServiceClientTracing.AddTracingInterceptor(traceInterceptor.Object);
            var wasTracingEnabled = ServiceClientTracing.IsEnabled;
            ServiceClientTracing.IsEnabled = true;

            await doTest();

            ServiceClientTracing.IsEnabled = wasTracingEnabled;

            var invocationId = invocationIds.Last();
            traceInterceptor.Verify(
                i => i.EnterMethod(invocationId, It.IsAny<object>(), tracedMethodName, It.IsAny<IDictionary<string, object>>()), Times.Once());
            traceInterceptor.Verify(
                i => i.SendRequest(invocationId, It.IsAny<HttpRequestMessage>()), Times.Once());
            traceInterceptor.Verify(
                i => i.ReceiveResponse(invocationId, It.IsAny<HttpResponseMessage>()), Times.Once());

            if (isSuccesful)
            {
                traceInterceptor.Verify(
                    i => i.ExitMethod(invocationId, It.IsAny<HttpOperationResponse>()), Times.Once());
            }
            else
            {
                traceInterceptor.Verify(
                    i => i.TraceError(invocationId, It.IsAny<Exception>()), Times.Once());
            }

            if (assertHttpRequestMessage != null)
            {
                traceInterceptor.Verify(
                    i => i.SendRequest(invocationId, It.Is<HttpRequestMessage>(h => assertHttpRequestMessage(h))), "HttpRequestMessage does not validate condition.");
            }
        }

        public async Task UseClientFor(Func<IConnectorClient, Task> doTest, string className = null, [CallerMemberName] string methodName = null)
        {
            using (var context = MockContext.Start(className ?? ClassName, methodName))
            {
                HttpMockServer.Initialize(className ?? ClassName, methodName, mode);

                using (var client = new ConnectorClient(HostUri, new BotAccessTokenStub(token), handlers: HttpMockServer.CreateInstance()))
                {
                    await doTest(client);
                }

                context.Stop();
            }
        }

        public async Task UseOAuthClientFor(Func<OAuthClient, Task> doTest, string className = null, [CallerMemberName] string methodName = null)
        {
            using (var context = MockContext.Start(className ?? ClassName, methodName))
            {
                HttpMockServer.Initialize(className ?? ClassName, methodName, mode);
                using (var oauthClient = new OAuthClient(new Uri(AuthenticationConstants.OAuthUrl), new BotAccessTokenStub(token), handlers: HttpMockServer.CreateInstance()))
                {
                    await doTest(oauthClient);
                }

                if (mode == HttpRecorderMode.Record)
                {
                    HttpMockServer.FileSystemUtilsObject = new FileSystemUtils();
                    HttpMockServer.Flush();
                }

                context.Stop();
            }
        }
    }
}
