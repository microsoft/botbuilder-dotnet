using System;
using System.IO;
using Antlr4.Runtime;
using Xunit;
using static Microsoft.Bot.Builder.LanguageGeneration.LGTemplateParser;

namespace Microsoft.Bot.Builder.LanguageGeneration.Tests
{
    public class TemplatesParseTreeTest
    {
        [Fact]
        public void ParseTreeTest()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("ParseTreeTest.lg"));

            Assert.Equal(4, templates.Count);

            // Normal template body
            var normalTemplateBody = templates[0].TemplateBodyParseTree as NormalBodyContext;

            // - ${welcomeword} ${name}
            Assert.Equal("-${welcomeword} ${name}", normalTemplateBody.GetText());
            var templateStrings = normalTemplateBody.normalTemplateBody().templateString();
            Assert.Equal(1, templateStrings.Length);
            var expressions = templateStrings[0].normalTemplateString().expression();
            Assert.Equal("${welcomeword}", expressions[0].GetText());
            Assert.Equal("${name}", expressions[1].GetText());

            // Condition template body
            var conditionalBody = templates[1].TemplateBodyParseTree as IfElseBodyContext;
            var rules = conditionalBody.ifElseTemplateBody().ifConditionRule();
            Assert.Equal(3, rules.Length);

            // - IF:${a > 0}
            // -positve
            var ifCondition = rules[0].ifCondition();
            Assert.Equal("-IF:${a > 0}", ifCondition.GetText());
            var expressionContext = ifCondition.expression()[0];
            Assert.Equal("${a > 0}", expressionContext.GetText());

            //-ELSEIF: ${ a == 0}
            //-equals to 0
            var elseIfCondition = rules[1].ifCondition();
            Assert.Equal("-ELSEIF:${a == 0}", elseIfCondition.GetText());
            expressionContext = elseIfCondition.expression()[0];
            Assert.Equal("${a == 0}", expressionContext.GetText());

            // - ELSE:
            // - negative
            var elseCondition = rules[2].ifCondition();
            Assert.Equal("-ELSE:", elseCondition.GetText());

            // switch/case template body
            var switchBody = templates[2].TemplateBodyParseTree as SwitchCaseBodyContext;
            var caseRules = switchBody.switchCaseTemplateBody().switchCaseRule();
            Assert.Equal(4, caseRules.Length);

            // -SWITCH:${day}
            var switchStat = caseRules[0].switchCaseStat();
            Assert.Equal("-SWITCH:${day}", switchStat.GetText());
            expressionContext = switchStat.expression()[0];
            Assert.Equal("${day}", expressionContext.GetText());

            //-CASE: ${'Saturday'}
            //-Happy Saturday!
            var caseStat = caseRules[1].switchCaseStat();
            Assert.Equal("-CASE:${'Saturday'}", caseStat.GetText());
            expressionContext = caseStat.expression()[0];
            Assert.Equal("${'Saturday'}", expressionContext.GetText());

            //-DEFAULT:
            var defaultStat = caseRules[3].switchCaseStat();
            Assert.Equal("-DEFAULT:", defaultStat.GetText());

            // structure
            var structureBody = templates[3].TemplateBodyParseTree as StructuredBodyContext;
            var nameLine = structureBody.structuredTemplateBody().structuredBodyNameLine();
            Assert.Equal("MyStruct", nameLine.STRUCTURE_NAME().GetText());
            var bodyLines = structureBody.structuredTemplateBody().structuredBodyContentLine();
            Assert.Equal(1, bodyLines.Length);

            //body =${ body}
            var contentLine = bodyLines[0].keyValueStructureLine();
            Assert.Equal("body", contentLine.STRUCTURE_IDENTIFIER().GetText());
            Assert.Equal(1, contentLine.keyValueStructureValue().Length);
            Assert.Equal("${body}", contentLine.keyValueStructureValue()[0].expressionInStructure()[0].GetText());
        }

        private string GetExampleFilePath(string fileName)
        {
            return Path.Combine(AppContext.BaseDirectory, "Examples", fileName);
        }
    }
}
