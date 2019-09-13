// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Form.Tests
{
    public class GeneratorTests : IClassFixture<FormFixture>
    {
        FormFixture _form;

        public GeneratorTests(FormFixture form)
        {
            _form = form;
        }

        private readonly string samplesDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Form.Tests\Resources\");

        [Fact]
        public async Task TestAsk()
        {
            await BuildTestFlow("TestAsk", @"sandwich.dialog")
                .AssertReply("Welcome!")
                .Send("Order a ham sandwich")
            .StartTestAsync();
        }

        private TestFlow BuildTestFlow(string testName, string resourceName, bool sendTrace = false)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();
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

    }

    public class FormFixture : IDisposable
    {
        public FormFixture()
        {
            TypeFactory.Configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var projPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath($@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Form.Tests\Microsoft.Bot.Builder.Dialogs.Form.Tests.csproj")));
            Resources = ResourceExplorer.LoadProject(projPath);
        }

        public void Dispose()
        {
            Resources.Dispose();
        }

        public ResourceExplorer Resources { get; }
    }
}
