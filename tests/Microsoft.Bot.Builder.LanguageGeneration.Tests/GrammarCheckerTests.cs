using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.LanguageGeneration.GrmCheckers;
using System.Linq;
using System.IO;

namespace Microsoft.Bot.Builder.LanguageGeneration.Tests
{
    [TestClass]
    public class GrammarCheckerTests
    {
        private string GetExampleFilePath(string fileName)
        {
            return Path.Combine(AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")), "Examples", fileName);
        }

        [TestMethod]
        public void TestVoidGrammarChecker()
        {
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("2.lg"));
            engine.Middlewares.Add(new VoidChecker());

            var evaled = engine.EvaluateTemplate("wPhrase", null);
            var options = new List<string> { "Hi", "Hello", "Hiya " };

            Assert.IsTrue(options.Contains(evaled), $"The result `{evaled}` is not in those options [{string.Join(",", options)}]");
        }
    }
}
