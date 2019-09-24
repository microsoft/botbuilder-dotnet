// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.MockLuis;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Form.Tests
{
    public class GeneratorTests : IClassFixture<GeneratorTests.FormFixture>
    {
        private FormFixture _form;

        private readonly string resourcesDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Form.Tests\Resources\");

        public GeneratorTests(FormFixture form)
        {
            _form = form;
        }

        [Fact]
        public async Task TestAsk()
        {
            await BuildTestFlow("TestAsk", @"sandwich.main.dialog")
                .Send("Order a ham sandwich")
            .StartTestAsync();
        }

        private TestFlow BuildTestFlow(string testName, string resourceName, bool sendTrace = false)
        {
            TypeFactory.Configuration = new ConfigurationBuilder()
                .UseLuisSettings(resourcesDirectory, "formTests")
                .Build();
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);
            var adapter = new TestAdapter(TestAdapter.CreateConversation(testName), sendTrace);
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .UseResourceExplorer(_form.Resources)
                .UseAdaptiveDialogs()
                .UseFormDialogs()
                .UseLanguageGeneration(_form.Resources)
                .UseMockLuis()
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var resource = _form.Resources.GetResource(resourceName);
            if (resource == null)
            {
                throw new Exception($"Resource[{resourceName}] not found");
            }

            var dialog = DeclarativeTypeLoader.Load<Dialog>(resource, _form.Resources, DebugSupport.SourceRegistry);
            var dm = new DialogManager(dialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }

        public class FormFixture : IDisposable
        {
            public FormFixture()
            {
                TypeFactory.Configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
                var projPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath($@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Form.Tests\Microsoft.Bot.Builder.Dialogs.Form.Tests.csproj")));
                Resources = ResourceExplorer.LoadProject(projPath);
            }

            public ResourceExplorer Resources { get; }

            public void Dispose()
            {
                Resources.Dispose();
            }
        }
    }
}
