using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class TemplateEngineThrowExceptionTest
    {
        public static IEnumerable<object[]> StaticCheckExceptionData => new[]
        {
            Test("EmptyTemplate.lg"),
            Test("ErrorTemplateParameters.lg"),
            Test("NoNormalTemplateBody.lg"),
            Test("ConditionFormatError.lg"),
            Test("NoTemplateRef.lg"),
            Test("TemplateParamsNotMatchArgsNum.lg"),
            Test("ErrorSeperateChar.lg"),
            Test("ErrorSeperateChar2.lg"),
            Test("MultilineVariation.lg"),
            Test("InvalidTemplateName.lg"),
            Test("InvalidTemplateName2.lg"),
            Test("DuplicatedTemplates.lg"),
            Test("LgTemplateFunctionError.lg"),
            Test("SwitchCaseFormatError.lg"),
            Test("InvalidLGFileImportPath.lg"),
            Test("DuplicatedTemplatesInImportFiles.lg"),
            Test("ErrorStructuredTemplate.lg"),
        };

        public static IEnumerable<object[]> StaticCheckWariningData => new[]
        {
            Test("EmptyLGFile.lg"),
            Test("OnlyNoMatchRule.lg"),
            Test("NoMatchRule.lg"),
            Test("SwitchCaseWarning.lg")
        };

        public static IEnumerable<object[]> AnalyzerExceptionData => new[]
        {
            TestTemplate("LoopDetected.lg", "NotExistTemplateName"),
            TestTemplate("LoopDetected.lg", "wPhrase"),
        };

        public static IEnumerable<object[]> EvaluatorExceptionData => new[]
        {
            TestTemplate("ErrorExpression.lg", "template1"),
            TestTemplate("LoopDetected.lg", "wPhrase"),
            TestTemplate("LoopDetected.lg", "NotExistTemplate"),
        };

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        /// </summary>
        /// <value>
        /// The test context which provides
        ///  information about and functionality for the current test run.
        /// </value>
        public TestContext TestContext { get; set; }

        public static object[] Test(string input) => new object[] { input };

        public static object[] TestTemplate(string input, string templateName) => new object[] { input, templateName };

        [DataTestMethod]
        [DynamicData(nameof(StaticCheckExceptionData))]
        public void ThrowExceptionTest(string input)
        {
            var isFail = false;
            try
            {
                new TemplateEngine().AddFile(GetExampleFilePath(input));
                isFail = true;
            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.Message);
            }

            if (isFail)
            {
                Assert.Fail("No exception is thrown.");
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(StaticCheckWariningData))]
        public void WariningTest(string input)
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath(input));

            var report = new StaticChecker().CheckTemplates(engine.Templates);

            TestContext.WriteLine(string.Join("\n", report));
        }

        [DataTestMethod]
        [DynamicData(nameof(AnalyzerExceptionData))]
        public void AnalyzerThrowExceptionTest(string input, string templateName)
        {
            var isFail = false;
            var errorMessage = string.Empty;
            TemplateEngine engine = null;
            try
            {
                engine = new TemplateEngine().AddFile(GetExampleFilePath(input));
            }
            catch (Exception)
            {
                isFail = true;
                errorMessage = "error occurs when parsing file";
            }

            if (!isFail)
            {
                try
                {
                    engine.AnalyzeTemplate(templateName);
                    isFail = true;
                    errorMessage = "No exception is thrown.";
                }
                catch (Exception e)
                {
                    TestContext.WriteLine(e.Message);
                }
            }

            if (isFail)
            {
                Assert.Fail(errorMessage);
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(EvaluatorExceptionData))]
        public void EvaluatorThrowExceptionTest(string input, string templateName)
        {
            var isFail = false;
            var errorMessage = string.Empty;
            TemplateEngine engine = null;
            try
            {
                engine = new TemplateEngine().AddFile(GetExampleFilePath(input));
            }
            catch (Exception)
            {
                isFail = true;
                errorMessage = "error occurs when parsing file";
            }

            if (!isFail)
            {
                try
                {
                    engine.EvaluateTemplate(templateName, null);
                    isFail = true;
                    errorMessage = "No exception is thrown.";
                }
                catch (Exception e)
                {
                    TestContext.WriteLine(e.Message);
                }
            }

            if (isFail)
            {
                Assert.Fail(errorMessage);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void AddTextWithWrongId()
        {
            new TemplateEngine().AddText("# t \n - hi", "a.lg");
        }

        private string GetExampleFilePath(string fileName)
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "ExceptionExamples" + Path.DirectorySeparatorChar + fileName;
        }
    }
}
