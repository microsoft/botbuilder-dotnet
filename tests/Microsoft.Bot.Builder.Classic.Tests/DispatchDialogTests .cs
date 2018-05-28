using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Luis;
using Microsoft.Bot.Builder.Classic.Luis.Models;
using Microsoft.Bot.Builder.Classic.Scorables;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Match = System.Text.RegularExpressions.Match;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public sealed class DispatchDialogTests : DialogTestBase
    {
        private sealed class TestNextGroupDialog : DispatchDialog
        {
            [RegexPattern("hello")]
            [ScorableGroup(1)]
            public async Task MatchedLessGroupOne(IDialogContext context, IMessageActivity message)
            {
                if (message.Text.Contains("skip"))
                {
                    this.ContinueWithNextGroup();
                }
                else
                {
                    await context.PostAsync("matched less, group one");
                }
            }

            [RegexPattern("hello world")]
            [ScorableGroup(1)]
            public async Task MatchedMoreGroupOne(IDialogContext context, IMessageActivity message)
            {
                if (message.Text.Contains("skip"))
                {
                    this.ContinueWithNextGroup();
                }
                else
                {
                    await context.PostAsync("matched more, group one");
                }
            }

            [RegexPattern("hello")]
            [ScorableGroup(2)]
            public async Task MatchedLessGroupTwo(IDialogContext context, IMessageActivity message)
            {
                await context.PostAsync("matched less, group two");
            }

            [RegexPattern("hello world")]
            [ScorableGroup(2)]
            public async Task MatchedMoreGroupTwo(IDialogContext context, IMessageActivity message)
            {
                await context.PostAsync("matched more, group two");
            }

            [MethodBind]
            [ScorableGroup(3)]
            public async Task MatchDefault(IDialogContext context, IMessageActivity message)
            {
                await context.PostAsync($"echo: {message.Text}");
            }
        }

        [TestMethod]
        public async Task Dispatch_NextDispatchGroup()
        {
            using (var container = Build(Options.ResolveDialogFromContainer | Options.Reflection))
            using (var scope = container.BeginLifetimeScope(
                builder => builder.RegisterType<TestNextGroupDialog>().As<IDialog<object>>()))
            {
                await AssertScriptAsync(scope,
                    "start",
                    "echo: start",
                    "hello",
                    "matched less, group one",
                    "hello world",
                    "matched more, group one",
                    "hello skip",
                    "matched less, group two",
                    "hello world skip",
                    "matched more, group two",
                    "after",
                    "echo: after"
                    );
            }
        }
    }

    [TestClass]
    public sealed class DispatchDialogMethodsTests : DispatchTestsBase
    {
        [Serializable]
        private class TestDispatchDialog : DispatchDialog<object>, IMethods
        {
            [NonSerialized]
            private readonly Func<ILuisModel, ILuisService> MakeLuisService;
            // inner mocked IMethods to verify this dialog's methods are being called
            private readonly IMethods methods;
            public TestDispatchDialog(Func<ILuisModel, ILuisService> MakeLuisService, IMethods methods)
            {
                SetField.NotNull(out this.MakeLuisService, nameof(MakeLuisService), MakeLuisService);
                SetField.NotNull(out this.methods, nameof(methods), methods);
            }

            protected override ILuisService MakeService(ILuisModel model)
            {
                return MakeLuisService(model);
            }

            public Task Activity(IActivity activity)
            {
                return methods.Activity(activity);
            }

            public Task Activity(ITypingActivity activity)
            {
                return methods.Activity(activity);
            }

            public Task Activity(IMessageActivity activity)
            {
                return methods.Activity(activity);
            }

            public Task LuisAllTypes(
                ILuisModel model,
                IntentRecommendation intent,
                LuisResult result,
                [Entity("entityTypeA")] string entityA_as_String,
                [Entity("entityTypeA")] IEnumerable<string> entityA_as_IEnumerable_String,
                [Entity("entityTypeA")] IReadOnlyCollection<string> entityA_as_IReadOnlyCollection_String,
                [Entity("entityTypeA")] IReadOnlyList<string> entityA_as_IReadOnlyList_String,
                [Entity("entityTypeA")] EntityRecommendation entityA_as_EntityRecommendation,
                [Entity("entityTypeA")] IEnumerable<EntityRecommendation> entityA_as_IEnumerable_EntityRecommendation,
                [Entity("entityTypeA")] IReadOnlyCollection<EntityRecommendation> entityA_as_IReadOnlyCollection_EntityRecommendation,
                [Entity("entityTypeA")] IReadOnlyList<EntityRecommendation> entityA_as_IReadOnlyList_EntityRecommendation,
                [Entity("entityTypeB")] string entityB_as_String,
                [Entity("entityTypeB")] IEnumerable<string> entityB_as_IEnumerable_String,
                [Entity("entityTypeB")] IReadOnlyCollection<string> entityB_as_IReadOnlyCollection_String,
                [Entity("entityTypeB")] IReadOnlyList<string> entityB_as_IReadOnlyList_String,
                [Entity("entityTypeB")] EntityRecommendation entityB_as_EntityRecommendation,
                [Entity("entityTypeB")] IEnumerable<EntityRecommendation> entityB_as_IEnumerable_EntityRecommendation,
                [Entity("entityTypeB")] IReadOnlyCollection<EntityRecommendation> entityB_as_IReadOnlyCollection_EntityRecommendation,
                [Entity("entityTypeB")] IReadOnlyList<EntityRecommendation> entityB_as_IReadOnlyList_EntityRecommendation)
            {
                return methods.LuisAllTypes(model, intent, result, entityA_as_String, entityA_as_IEnumerable_String, entityA_as_IReadOnlyCollection_String, entityA_as_IReadOnlyList_String, entityA_as_EntityRecommendation, entityA_as_IEnumerable_EntityRecommendation, entityA_as_IReadOnlyCollection_EntityRecommendation, entityA_as_IReadOnlyList_EntityRecommendation, entityB_as_String, entityB_as_IEnumerable_String, entityB_as_IReadOnlyCollection_String, entityB_as_IReadOnlyList_String, entityB_as_EntityRecommendation, entityB_as_IEnumerable_EntityRecommendation, entityB_as_IReadOnlyCollection_EntityRecommendation, entityB_as_IReadOnlyList_EntityRecommendation);
            }

            public Task LuisNone(ILuisModel model)
            {
                return methods.LuisNone(model);
            }

            public Task LuisOne(
                ILuisModel model,
                [Entity("entityTypeA")] IEnumerable<string> entityA)
            {
                return methods.LuisOne(model, entityA);
            }

            public Task LuisTwo(
                ILuisModel model,
                [Entity("entityTypeA")] string entityA)
            {
                return methods.LuisTwo(model, entityA);
            }

            public Task RegexAllTypes(
                Regex regex,
                Match match,
                CaptureCollection captures,
                [Entity("captureAll")] Capture capture,
                [Entity("captureAll")] string text)
            {
                return methods.RegexAllTypes(regex, match, captures, capture, text);
            }

            public Task RegexOne(
                [Entity("captureOne")] Capture capture)
            {
                return methods.RegexOne(capture);
            }

            public Task RegexTwo(
                [Entity("captureTwo")] string capture)
            {
                return methods.RegexTwo(capture);
            }
        }

        private readonly ILifetimeScope container;

        public DispatchDialogMethodsTests()
        {
            this.container = DialogTestBase.Build(
                DialogTestBase.Options.None | DialogTestBase.Options.ResolveDialogFromContainer,
                this.methods.Object, this.luisOne.Object, this.luisTwo.Object)
                .BeginLifetimeScope(
                builder =>
                {
                    builder
                        .RegisterInstance(this.methods.Object)
                        .As<IMethods>();
                    builder
                        .RegisterInstance<Func<ILuisModel, ILuisService>>(MakeLuisService)
                        .AsSelf();
                    builder
                        .RegisterType<TestDispatchDialog>()
                        .As<IDialog<object>>()
                        .InstancePerLifetimeScope();
                });
        }

        public override async Task ActAsync()
        {
            await new TestAdapter().ProcessActivity((Activity)this.activity, async (context) =>
            {
                using (var scope = DialogModule.BeginLifetimeScope(this.container, context))
                {
                    var task = scope.Resolve<IPostToBot>();
                    await task.PostAsync(this.activity, this.token);
                }
            });
        }
    }
}