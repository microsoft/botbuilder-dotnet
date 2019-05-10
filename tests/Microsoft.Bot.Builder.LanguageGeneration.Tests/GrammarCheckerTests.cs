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
        public GrammarChecker gc  = null;
        public OutputTransformationContext context = null;

        [TestInitialize]
        public void TestInitialization()
        {
            gc = new GrammarChecker();
            gc.Init();
            context = new OutputTransformationContext();
        }

        //private string GetExampleFilePath(string fileName)
        //{
        //    return Path.Combine(AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")), "Examples", fileName);
        //}

        //[TestMethod]
        //public void TestVoidGrammarChecker()
        //{
        //    var engine = TemplateEngine.FromFiles(GetExampleFilePath("2.lg"));
        //    engine.OutputTransformers.Add(new VoidChecker());

        //    var evaled = engine.EvaluateTemplate("wPhrase", null);
        //    var options = new List<string> { "Hi", "Hello", "Hiya " };

        //    Assert.IsTrue(options.Contains(evaled), $"The result `{evaled}` is not in those options [{string.Join(",", options)}]");
        //}

        [TestMethod]
        public void TestNumNounGrammarChecker()
        {
            string orgin_sent = "It's about 12 mile away";
            string truth_sent = "It's about 12 miles away";
            string result = gc.Transform(orgin_sent, context);
            Assert.AreEqual(truth_sent, result);
        }

        [TestMethod]
        public void TestAOrAnGrammarChecker()
        {
            string orgin_sent = "An useful tool"; 
            string truth_sent = "A useful tool";
            string result = gc.Transform(orgin_sent, context);
            Assert.AreEqual(truth_sent, result);
        }

        [TestMethod]
        public void TestSubjectVerbConsistencyGrammarChecker()
        {
            string orgin_sent = "there is 54 cheap restaurants.";
            string truth_sent = "there are 54 cheap restaurants.";
            string result = gc.Transform(orgin_sent, context);
            Assert.AreEqual(truth_sent, result);
        }
    }
}
