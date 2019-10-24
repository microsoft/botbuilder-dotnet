// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
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
        public SettingsStateTests()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

            this.Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task DialogContextState_SettingsTest()
        {
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
            await CreateFlow("en-us", dialog)
                .Send("howdy")
                    .AssertReply("00000000-0000-0000-0000-000000000000")
                .Send("howdy")
                    .AssertReply("00000000-0000-0000-0000-000000000000")
                .Send("howdy")
                    .AssertReply("00000000-0000-0000-0000-000000000000")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task TestTurnStateAcrossBoundaries()
        {
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>
                    {
                        new IntentPattern() { Intent = "Test", Pattern = "test" }
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnIntent()
                    {
                        Intent = "Test",
                        Actions = new List<Dialog>()
                        {
                            new SetProperty() { Property = "dialog.name", Value = "'foo'" },
                            new TextInput() { Prompt = new ActivityTemplate("what is your name?"), Property = "dialog.name" },
                            new SendActivity() { Activity = new ActivityTemplate("{turn.recognized.intent}") }
                        }
                    }
                }
            };
            var rootDialog2 = new AdaptiveDialog()
            {
                Triggers = new List<OnCondition>()
                {
                    new OnBeginDialog()
                    {
                        Actions = new List<Dialog>()
                        {
                            new BeginDialog() { Dialog = rootDialog }
                        }
                    }
                }
            };

            await CreateFlow("en-us", rootDialog)
                .Send("test")
                    .AssertReply("Test")
            .StartTestAsync();
        }

        private TestFlow CreateFlow(string locale, Dialog dialog)
        {
            TypeFactory.Configuration = this.Configuration;

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
