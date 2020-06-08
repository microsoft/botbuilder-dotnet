using System;
using System.IO;
using Antlr4.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Bot.Builder.LanguageGeneration.LGTemplateParser;

namespace Microsoft.Bot.Builder.LanguageGeneration.Tests
{
    [TestClass]
    public class TemplatesParseTreeTest
    {
        [TestMethod]
        public void ParseTreeTest()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("ParseTreeTest.lg"));

            Assert.AreEqual(4, templates.Count);

            // Normal template body
            var normalTemplateBody = templates[0].TemplateBodyParseTree as NormalBodyContext;

            // - ${welcomeword} ${name}
            Assert.AreEqual("-${welcomeword} ${name}", normalTemplateBody.GetText());
            var templateStrings = normalTemplateBody.normalTemplateBody().templateString();
            Assert.AreEqual(1, templateStrings.Length);
            var expressions = templateStrings[0].normalTemplateString().expression();
            Assert.AreEqual("${welcomeword}", expressions[0].GetText());
            Assert.AreEqual("${name}", expressions[1].GetText());

            // Condition template body
            var conditionalBody = templates[1].TemplateBodyParseTree as IfElseBodyContext;
            var rules = conditionalBody.ifElseTemplateBody().ifConditionRule();
            Assert.AreEqual(3, rules.Length);

            // - IF:${a > 0}
            // -positve
            var ifCondition = rules[0].ifCondition();
            Assert.AreEqual("-IF:${a > 0}", ifCondition.GetText());
            var expressionContext = ifCondition.expression()[0];
            Assert.AreEqual("${a > 0}", expressionContext.GetText());

            //-ELSEIF: ${ a == 0}
            //-equals to 0
            var elseIfCondition = rules[1].ifCondition();
            Assert.AreEqual("-ELSEIF:${a == 0}", elseIfCondition.GetText());
            expressionContext = elseIfCondition.expression()[0];
            Assert.AreEqual("${a == 0}", expressionContext.GetText());

            // - ELSE:
            // - negative
            var elseCondition = rules[2].ifCondition();
            Assert.AreEqual("-ELSE:", elseCondition.GetText());

            // switch/case template body
            var switchBody = templates[2].TemplateBodyParseTree as SwitchCaseBodyContext;
            var caseRules = switchBody.switchCaseTemplateBody().switchCaseRule();
            Assert.AreEqual(4, caseRules.Length);

            // -SWITCH:${day}
            var switchStat = caseRules[0].switchCaseStat();
            Assert.AreEqual("-SWITCH:${day}", switchStat.GetText());
            expressionContext = switchStat.expression()[0];
            Assert.AreEqual("${day}", expressionContext.GetText());

            //-CASE: ${'Saturday'}
            //-Happy Saturday!
            var caseStat = caseRules[1].switchCaseStat();
            Assert.AreEqual("-CASE:${'Saturday'}", caseStat.GetText());
            expressionContext = caseStat.expression()[0];
            Assert.AreEqual("${'Saturday'}", expressionContext.GetText());

            //-DEFAULT:
            var defaultStat = caseRules[3].switchCaseStat();
            Assert.AreEqual("-DEFAULT:", defaultStat.GetText());

            // structure
            var structureBody = templates[3].TemplateBodyParseTree as StructuredBodyContext;
            var nameLine = structureBody.structuredTemplateBody().structuredBodyNameLine();
            Assert.AreEqual("MyStruct", nameLine.STRUCTURE_NAME().GetText());
            var bodyLines = structureBody.structuredTemplateBody().structuredBodyContentLine();
            Assert.AreEqual(1, bodyLines.Length);

            //body =${ body}
            var contentLine = bodyLines[0].keyValueStructureLine();
            Assert.AreEqual("body", contentLine.STRUCTURE_IDENTIFIER().GetText());
            Assert.AreEqual(1, contentLine.keyValueStructureValue().Length);
            Assert.AreEqual("${body}", contentLine.keyValueStructureValue()[0].expressionInStructure()[0].GetText());
        }

        private string GetExampleFilePath(string fileName)
        {
            return Path.Combine(AppContext.BaseDirectory, "Examples", fileName);
        }
    }
}
