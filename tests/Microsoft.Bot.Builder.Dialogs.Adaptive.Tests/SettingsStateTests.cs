// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable SA1401 // Fields should be private

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class SettingsStateTests
    {
        public IConfiguration Configuration;

        public SettingsStateTests()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

            this.Configuration = builder.Build();
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task DialogContextState_SettingsTest()
        {
            await CreateFlow("en-us")
                .Send("howdy")
                    .AssertReply("00000000-0000-0000-0000-000000000000")
                .Send("howdy")
                    .AssertReply("00000000-0000-0000-0000-000000000000")
                .Send("howdy")
                    .AssertReply("00000000-0000-0000-0000-000000000000")
                .StartTestAsync();
        }

        private TestFlow CreateFlow(string locale)
        {
            TypeFactory.Configuration = this.Configuration;
            var dialog = new AdaptiveDialog();
            dialog.Triggers.AddRange(new List<OnCondition>()
            {
                new OnUnknownIntent(actions:
                    new List<Dialog>()
                    {
                        new SendActivity()
                        {
                            Activity = new ActivityTemplate("{settings.ApplicationInsights.InstrumentationKey}")
                        },
                    }),
            });

            var resourceExplorer = new ResourceExplorer();

            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName))
                .Use(new RegisterClassMiddleware<ResourceExplorer>(resourceExplorer))
                .Use(new RegisterClassMiddleware<IStorage>(new MemoryStorage()))
                .UseAdaptiveDialogs()
                .UseLanguageGeneration(resourceExplorer)
                .Use(new RegisterClassMiddleware<IConfiguration>(this.Configuration))
                .UseState(new UserState(new MemoryStorage()), new ConversationState(new MemoryStorage()))
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            DialogManager dm = new DialogManager(dialog);

            return new TestFlow((TestAdapter)adapter, async (turnContext, cancellationToken) =>
            {
                await dm.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            });
        }
    }
}
