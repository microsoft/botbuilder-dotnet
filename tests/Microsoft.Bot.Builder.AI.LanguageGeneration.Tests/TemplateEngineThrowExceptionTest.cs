using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Checker;
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

        public static IEnumerable<object[]> Data => new[]
       {
            Test("EmptyTemplate.lg"),
        };


        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void ThrowExceptionTest(string input)
        {
            try
            {
                TemplateEngine.FromFile(GetExampleFilePath(input));
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(LGParsingException));
                TestContext.WriteLine(e.Message);
            }
        }
    }
}
