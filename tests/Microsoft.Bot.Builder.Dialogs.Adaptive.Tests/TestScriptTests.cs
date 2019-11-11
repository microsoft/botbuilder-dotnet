// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
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
        public static ResourceExplorer ResourceExplorer { get; set; } = new ResourceExplorer();

        public static IEnumerable<object[]> TestScripts { get; set; }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            ResourceExplorer.AddFolder(PathUtils.NormalizePath(@"..\..\.."));
            TypeFactory.Configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            DeclarativeTypeLoader.AddComponent(new DialogComponentRegistration());
            DeclarativeTypeLoader.AddComponent(new AdaptiveComponentRegistration());
            DeclarativeTypeLoader.AddComponent(new LanguageGenerationComponentRegistration());
            DeclarativeTypeLoader.AddComponent(new QnAMakerComponentRegistration());
            TestScripts = ResourceExplorer.GetResources("testdialog").Select(resource => new object[] { resource.Id }).ToList();
        }

        [DataTestMethod]
        [DynamicData(nameof(TestScripts))]
        public async Task RunAllTestScripts(string resourceId)
        {
            await ResourceExplorer.LoadType<TestScript>(resourceId).ExecuteAsync(ResourceExplorer).ConfigureAwait(false);
        }

        //[TestMethod]
        //public async Task TestAssertReply()
        //{
        //    await ResourceExplorer.LoadType<TestScript>("TestAssertReply.testdialog").ExecuteAsync(ResourceExplorer);
        //}
    }
}
