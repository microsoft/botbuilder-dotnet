// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Templates.Tests
{
    [TestClass]
    public class ActivityCheckerTest
    {
        private static ResourceExplorer resourceExplorer;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            DeclarativeTypeLoader.AddComponent(new AdaptiveComponentRegistration());
            DeclarativeTypeLoader.AddComponent(new LanguageGenerationComponentRegistration());

            resourceExplorer = ResourceExplorer.LoadProject(GetProjectFolder());
        }

        [TestMethod]
        public void CheckOutPutNotFromStructuredLG()
        {
            var diagnostics = ActivityChecker.Check("Not a valid json");
            Assert.AreEqual(diagnostics.Count, 1);
            Assert.IsTrue(diagnostics[0].Severity == DiagnosticSeverity.Warning);
            Assert.AreEqual(diagnostics[0].Message, "LG output is not a json object, and will fallback to string format.");

            diagnostics = ActivityChecker.Check("{}");
            Assert.AreEqual(diagnostics.Count, 1);
            Assert.IsTrue(diagnostics[0].Severity == DiagnosticSeverity.Error);
            Assert.AreEqual(diagnostics[0].Message, "'type' or '$type' is not exist in lg output json object.");
        }

        [TestMethod]
        public async Task CheckStructuredLGErrors()
        {
            var context = await GetTurnContext("DignosticStructuredLG.lg");
            var languageGenerator = context.TurnState.Get<ILanguageGenerator>();

            var lgStringResult = await languageGenerator.Generate(context, "@{ErrorStructuredType()}", null);
            var diagnostics = ActivityChecker.Check(lgStringResult);
            Assert.AreEqual(diagnostics.Count, 1);
            Assert.IsTrue(diagnostics[0].Severity == DiagnosticSeverity.Error);
            Assert.AreEqual(diagnostics[0].Message, "Type 'mystruct' is not support currently.");

            lgStringResult = await languageGenerator.Generate(context, "@{ErrorActivityType()}", null);
            diagnostics = ActivityChecker.Check(lgStringResult);
            Assert.AreEqual(diagnostics.Count, 1);
            Assert.IsTrue(diagnostics[0].Severity == DiagnosticSeverity.Warning);
            Assert.AreEqual(diagnostics[0].Message, "'xxx' is not support currently. It will fallback to message activity.");

            lgStringResult = await languageGenerator.Generate(context, "@{ErrorMessage()}", null);
            diagnostics = ActivityChecker.Check(lgStringResult);
            Assert.AreEqual(diagnostics.Count, 6);
            Assert.IsTrue(diagnostics[0].Severity == DiagnosticSeverity.Warning);
            Assert.AreEqual(diagnostics[0].Message, "'attachment' is not support, do you mean 'attachments'?");
            Assert.IsTrue(diagnostics[1].Severity == DiagnosticSeverity.Warning);
            Assert.AreEqual(diagnostics[1].Message, "'suggestedaction' is not support, do you mean 'suggestedactions'?");
            Assert.IsTrue(diagnostics[2].Severity == DiagnosticSeverity.Warning);
            Assert.AreEqual(diagnostics[2].Message, "'mystruct' is not card action type.");
            Assert.IsTrue(diagnostics[3].Severity == DiagnosticSeverity.Error);
            Assert.AreEqual(diagnostics[3].Message, "'yyy' is not a valid action type");
            Assert.IsTrue(diagnostics[4].Severity == DiagnosticSeverity.Error);
            Assert.AreEqual(diagnostics[4].Message, "'notsure' is not a boolean value.");
            Assert.IsTrue(diagnostics[5].Severity == DiagnosticSeverity.Warning);
            Assert.AreEqual(diagnostics[5].Message, "'mystruct' is not an attachment type.");
        }

        private static string GetProjectFolder()
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
        }

        private async Task<ITurnContext> GetTurnContext(string lgFile)
        {
            var context = new TurnContext(new TestAdapter(), new Activity());
            var lgText = await resourceExplorer.GetResource(lgFile).ReadTextAsync();
            context.TurnState.Add<ILanguageGenerator>(new TemplateEngineLanguageGenerator(lgText, "test", LanguageGeneratorManager.MultiLanguageResolverDelegate(resourceExplorer)));
            return context;
        }
    }
}
