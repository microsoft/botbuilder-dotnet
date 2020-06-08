using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Bot.Builder.LanguageGeneration.LGTemplateParser;

namespace Microsoft.Bot.Builder.LanguageGeneration.Tests
{
    [TestClass]
    public class TemplatesParseTreeTest
    {
        [TestMethod]
        public void TestBasic()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("ParseTreeTest.lg"));

            Assert.AreEqual(4, templates.Count);

            // Normal template body
            Assert.AreEqual(1, templates[0].TemplateBodyParseTree.children.Count);
            var body = templates[0].TemplateBodyParseTree.children[0] as NormalBodyContext;

            Assert.AreEqual("-hi", body.GetText());
            Assert.AreEqual(1, body.Start.Line);
            Assert.AreEqual(0, body.Start.Column);
            Assert.AreEqual(1, body.Stop.Line);
            Assert.AreEqual(3, body.Stop.Column);

            // Condition template body
            Assert.AreEqual(1, templates[1].TemplateBodyParseTree.children.Count);
            var conditionBody = templates[1].TemplateBodyParseTree.children[0] as IfElseBodyContext;
            Assert.AreEqual(3, body.children.Count);

            // - IF:${a > 0}
            // -positve
            var ifCondition = body.children[0] as ParserRuleContext;
            Assert.AreEqual(2, ifCondition.children.Count);

            //var ifStatement = ifCondition.
        }

        private string GetExampleFilePath(string fileName)
        {
            return Path.Combine(AppContext.BaseDirectory, "Examples", fileName);
        }
    }
}
