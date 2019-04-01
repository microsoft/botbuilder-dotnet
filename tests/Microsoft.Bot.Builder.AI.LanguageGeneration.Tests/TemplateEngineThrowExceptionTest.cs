using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class TemplateEngineThrowExceptionTest
    {
        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private string GetExampleFilePath(string fileName)
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "ExceptionExamples\\" + fileName;
        }

        public static object[] Test(string input) => new object[] { input };

        public static IEnumerable<object[]> ExceptionData => new[]
        {
            Test("EmptyTemplate.lg"),
            Test("ErrorTemplateParameters.lg"),
            Test("NoNormalTemplateBody.lg"),
            Test("ConditionFormatError.lg"),
            Test("ErrorEscapeCharacter.lg"),
            Test("NoTemplateRef.lg"),
            Test("TemplateParamsNotMatchArgsNum.lg"),
            Test("ErrorSeperateChar.lg"),
            Test("MultilineVariation.lg"),
            Test("InvalidTemplateName.lg"),
        };

        public static IEnumerable<object[]> WariningData => new[]
        {
            Test("EmptyLGFile.lg"),
            Test("OnlyNoMatchRule.lg"),
            Test("NoMatchRule.lg")
        };


        [DataTestMethod]
        [DynamicData(nameof(ExceptionData))]
        public void ThrowExceptionTest(string input)
        {
            try
            {
                TemplateEngine.FromFile(GetExampleFilePath(input));
                //no exception, throw exception
                throw new Exception("No exception throw.");
            }
            catch (Exception e)
            {
                if(e is LGParsingException)
                {
                    TestContext.WriteLine(e.Message);
                }
                else
                {
                    throw e;
                }
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(WariningData))]
        public void WariningTest(string input)
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath(input));
        }
    }
}
