// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1201 // Elements should appear in the correct order

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class TestScriptTests
    {
        private static string rootFolder = PathUtils.NormalizePath(@"..\..\..");

        public static ResourceExplorer ResourceExplorer { get; set; } = new ResourceExplorer().AddFolder(rootFolder);

        public static IEnumerable<object[]> GetTestScripts(string relativeFolder)
        {
            string testFolder = Path.GetFullPath(Path.Combine(rootFolder, PathUtils.NormalizePath(relativeFolder)));
            return Directory.EnumerateFiles(testFolder, "*.test.dialog", SearchOption.AllDirectories).Select(s => new object[] { Path.GetFileName(s) }).ToArray();
        }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            DeclarativeTypeLoader.AddComponent(new DialogComponentRegistration());
            DeclarativeTypeLoader.AddComponent(new AdaptiveComponentRegistration());
            DeclarativeTypeLoader.AddComponent(new LanguageGenerationComponentRegistration());
            DeclarativeTypeLoader.AddComponent(new QnAMakerComponentRegistration());
        }

        public static IEnumerable<object[]> AllTestScripts => GetTestScripts(@".");

        [DataTestMethod]
        [DynamicData(nameof(AllTestScripts))]
        public async Task RunAllTestScripts(string resourceId)
        {
            await ResourceExplorer.LoadType<TestScript>(resourceId).ExecuteAsync(ResourceExplorer).ConfigureAwait(false);
        }

        public static IEnumerable<object[]> AsserplyReplyScripts => GetTestScripts(@"Tests\TestAssertReply");

        [DataTestMethod]
        [DynamicData(nameof(AsserplyReplyScripts))]
        public async Task TestScript_AssertReply(string resourceId)
        {
            await ResourceExplorer.LoadType<TestScript>(resourceId).ExecuteAsync(ResourceExplorer).ConfigureAwait(false);
        }

        public static IEnumerable<object[]> AssertReplyOneOfScripts => GetTestScripts(@"Tests\TestAssertReplyOneOf");

        [DataTestMethod]
        [DynamicData(nameof(AssertReplyOneOfScripts))]
        public async Task TestScript_AssertReplyOne(string resourceId)
        {
            await ResourceExplorer.LoadType<TestScript>(resourceId).ExecuteAsync(ResourceExplorer).ConfigureAwait(false);
        }

        public static IEnumerable<object[]> UserActionScripts => GetTestScripts(@"Tests\TestUser");

        [DataTestMethod]
        [DynamicData(nameof(UserActionScripts))]
        public async Task TestScript_UserAction(string resourceId)
        {
            await ResourceExplorer.LoadType<TestScript>(resourceId).ExecuteAsync(ResourceExplorer).ConfigureAwait(false);
        }

        public static IEnumerable<object[]> ActionTestsScripts => GetTestScripts(@"Tests\ActionTests");

        [DataTestMethod]
        [DynamicData(nameof(ActionTestsScripts))]
        public async Task ActionTests(string resourceId)
        {
            await ResourceExplorer.LoadType<TestScript>(resourceId).ExecuteAsync(ResourceExplorer).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task TestDialog()
        {
            await ResourceExplorer.LoadType<TestScript>("Action_EditActionReplaceSequence.test.dialog").ExecuteAsync(ResourceExplorer).ConfigureAwait(false);
        }
    }
}
