// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Tests
{
    public class TestUtils
    {
        public static IConfiguration DefaultConfiguration { get; set; } = new ConfigurationBuilder().AddInMemoryCollection().Build();

        public static string RootFolder { get; set; } = GetProjectPath();

        public static IEnumerable<object[]> GetTestScripts(string relativeFolder)
        {
            var testFolder = Path.GetFullPath(Path.Combine(RootFolder, PathUtils.NormalizePath(relativeFolder)));
            return Directory.EnumerateFiles(testFolder, "*.test.dialog", SearchOption.AllDirectories).Select(s => new object[] { Path.GetFileName(s) }).ToArray();
        }

        public static async Task RunTestScript(ResourceExplorer resourceExplorer, string resourceId = null, IConfiguration configuration = null, [CallerMemberName] string testName = null, HttpMessageHandler testHttpClientMessageHandler = null)
        {
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);

            var adapter = (TestAdapter)new TestAdapter(Channels.Msteams);
            if (testHttpClientMessageHandler != null)
            {
                var testHttpClient = new HttpClient(testHttpClientMessageHandler); 

                // Set a special base address so then we can make sure the connector client is honoring this http client
                testHttpClient.BaseAddress = new Uri("https://localhost.coffee");
                var testConnectorClient = new ConnectorClient(new Uri("http://localhost.coffee/"), new MicrosoftAppCredentials(string.Empty, string.Empty), testHttpClient);
                adapter.Use(new TestConnectorClientMiddleware(testConnectorClient));
            }

            adapter.Use(new RegisterClassMiddleware<IConfiguration>(DefaultConfiguration))
                .UseStorage(storage)
                .UseBotState(userState, convoState)
                .Use(new TranscriptLoggerMiddleware(new TraceTranscriptLogger(traceActivity: false)));

            adapter.OnTurnError += (context, err) => context.SendActivityAsync(err.Message);

            var script = resourceExplorer.LoadType<TestScript>(resourceId ?? $"{testName}.test.dialog");
            script.Configuration = configuration ?? new ConfigurationBuilder().AddInMemoryCollection().Build();
            script.Description ??= resourceId;
            await script.ExecuteAsync(adapter: adapter, testName: testName, resourceExplorer: resourceExplorer).ConfigureAwait(false);
        }

        public static string GetProjectPath()
        {
            var parent = Environment.CurrentDirectory;
            while (!string.IsNullOrEmpty(parent))
            {
                if (Directory.EnumerateFiles(parent, "*proj").Any())
                {
                    break;
                }

                parent = Path.GetDirectoryName(parent);
            }

            return parent;
        }

        private class TestConnectorClientMiddleware : IMiddleware
        {
            private IConnectorClient _connectorClient;

            public TestConnectorClientMiddleware(IConnectorClient connectorClient)
            {
                _connectorClient = connectorClient;
            }

            public Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
            {
                // turnContext.TurnState.Get<IConnectorClient>();
                turnContext.TurnState.Add<IConnectorClient>(_connectorClient);
                return next(cancellationToken);
            }
        }
    }
}
