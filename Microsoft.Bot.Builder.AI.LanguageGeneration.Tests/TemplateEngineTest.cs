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
            Assert.AreEqual(engine.Evaluate("wPhrase", null), "Hi", "");
        }
    }
}
