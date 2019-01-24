using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder.AI.LanguageGeneration;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class TemplateEngineTest
    {
        private string GetExampleFilePath(string fileName)
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "Examples\\" + fileName;
        }
        

        [TestMethod]
        public void TestBasic()
        {   
            var engine = TemplateEngine.FromFile(GetExampleFilePath("2.lg"));

            var evaled = engine.Evaluate("wPhrase", null);
            var options = new List<string> { "Hi", "Hello", "Hiya " };

            Assert.IsTrue(options.Contains(evaled), $"The result `{evaled}` is not in those options [{string.Join(",", options)}]");
        }

        [TestMethod]
        public void TestBasicTemplateReference()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("3.lg"));

            var evaled = engine.Evaluate("welcome-user", null);
            var options = new List<string> { "Hi", "Hello", "Hiya", "Hi :)", "Hello :)", "Hiya :)"};

            Assert.IsTrue(options.Contains(evaled), $"The result {evaled} is not in those options [{string.Join(",", options)}]");
        }

        [TestMethod]
        public void TestBasicTemplateRefAndEntityRef()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("4.lg"));

            var userName = "DL";
            var evaled = engine.Evaluate("welcome-user", new { userName = userName});
            var options = new List<string> { "Hi", "Hello", "Hiya ", "Hi :)", "Hello :)", "Hiya  :)" };

            Assert.IsTrue(evaled.Contains(userName),  $"The result {evaled} does not contiain `{userName}`");
        }

        [TestMethod]
        public void TestBaicConditionalTemplate()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("5.lg"));

            string evaled = engine.Evaluate("time-of-day-readout", new { timeOfDay = "morning" });
            Assert.IsTrue(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");

        }

    }
}
