using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Bot.Builder.LanguageGeneration;
using Xunit;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    public class TemplateDiagnosticTest
    {
        [Fact]
        public void TestConditionFormatError()
        {
            var diagnostics = GetDiagnostics("ConditionFormatError.lg");

            Assert.Equal(10, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[0].Severity);
            Assert.Contains(TemplateErrors.NotEndWithElseInCondition, diagnostics[0].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.Contains(TemplateErrors.InvalidExpressionInCondition, diagnostics[1].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.Contains(TemplateErrors.MultipleIfInCondition, diagnostics[2].Message);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[3].Severity);
            Assert.Contains(TemplateErrors.NotEndWithElseInCondition, diagnostics[3].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[4].Severity);
            Assert.Contains(TemplateErrors.ExtraExpressionInCondition, diagnostics[4].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[5].Severity);
            Assert.Contains(TemplateErrors.MultipleIfInCondition, diagnostics[5].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[6].Severity);
            Assert.Contains(TemplateErrors.InvalidMiddleInCondition, diagnostics[6].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[7].Severity);
            Assert.Contains(TemplateErrors.InvalidWhitespaceInCondition, diagnostics[7].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[8].Severity);
            Assert.Contains(TemplateErrors.InvalidWhitespaceInCondition, diagnostics[8].Message);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[9].Severity);
            Assert.Contains(TemplateErrors.NotStartWithIfInCondition, diagnostics[9].Message);
        }

        [Fact]
        public void TestDuplicatedTemplates()
        {
            var diagnostics = GetDiagnostics("DuplicatedTemplates.lg");

            Assert.Equal(2, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.Contains(TemplateErrors.DuplicatedTemplateInSameTemplate("template1"), diagnostics[0].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.Contains(TemplateErrors.DuplicatedTemplateInSameTemplate("template1"), diagnostics[1].Message);

            var lgFile = GetTemplates("DuplicatedTemplates.lg");
            var allDiagnostics = lgFile.AllDiagnostics;

            Assert.Equal(4, allDiagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, allDiagnostics[0].Severity);
            Assert.Contains(TemplateErrors.DuplicatedTemplateInSameTemplate("template1"), allDiagnostics[0].Message);
            Assert.Equal(DiagnosticSeverity.Error, allDiagnostics[1].Severity);
            Assert.Contains(TemplateErrors.DuplicatedTemplateInSameTemplate("template1"), allDiagnostics[1].Message);
            Assert.Equal(DiagnosticSeverity.Error, allDiagnostics[2].Severity);
            Assert.Contains("Duplicated definitions found for template: 'basicTemplate'", allDiagnostics[2].Message);
            Assert.Equal(DiagnosticSeverity.Error, allDiagnostics[3].Severity);
            Assert.Contains("Duplicated definitions found for template: 'basicTemplate2'", allDiagnostics[3].Message);
        }

        [Fact]
        public void TestDuplicatedTemplatesInImportFiles()
        {
            var diagnostics = GetDiagnostics("DuplicatedTemplatesInImportFiles.lg");

            Assert.Equal(2, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.Contains("Duplicated definitions found for template: 'basicTemplate'", diagnostics[0].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.Contains("Duplicated definitions found for template: 'basicTemplate2'", diagnostics[1].Message);
        }

        [Fact]
        public void TestEmptyLGFile()
        {
            var diagnostics = GetDiagnostics("EmptyLGFile.lg");

            Assert.Equal(1, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[0].Severity);
            Assert.Contains(TemplateErrors.NoTemplate, diagnostics[0].Message);
        }

        [Fact]
        public void TestEmptyTemplate()
        {
            var diagnostics = GetDiagnostics("EmptyTemplate.lg");

            Assert.Equal(1, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[0].Severity);
            Assert.Contains(TemplateErrors.NoTemplateBody("template"), diagnostics[0].Message);
        }

        [Fact]
        public void TestErrorStructuredTemplate()
        {
            var diagnostics = GetDiagnostics("ErrorStructuredTemplate.lg");

            Assert.Equal(8, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.Contains(TemplateErrors.InvalidStrucBody("abc"), diagnostics[0].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.Contains(TemplateErrors.EmptyStrucContent, diagnostics[1].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.Contains("Error occurred when parsing expression 'NOTemplate()'. NOTemplate does not have an evaluator", diagnostics[2].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[3].Severity);
            Assert.Contains("Error occurred when parsing expression 'NOTemplate()'. NOTemplate does not have an evaluator", diagnostics[3].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[4].Severity);
            Assert.Contains(TemplateErrors.InvalidStrucName("Activity%"), diagnostics[4].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[5].Severity);
            Assert.Contains(TemplateErrors.InvalidStrucName("Activity]"), diagnostics[5].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[6].Severity); 
            Assert.Contains(TemplateErrors.MissingStrucEnd, diagnostics[6].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[7].Severity);
            Assert.Contains(TemplateErrors.InvalidStrucBody("- hi"), diagnostics[7].Message);
        }

        [Fact]
        public void TestErrorMultiLineExpr()
        {
            var diagnostics = GetDiagnostics("MultiLineExprError.lg");

            Assert.Equal(1, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.Contains("Close } is missing in Expression", diagnostics[0].Message);

            diagnostics = Templates.ParseResource(new LGResource(string.Empty, string.Empty, "#Demo2\r\n- ${createArray(1,\r\n, 2,3)")).Diagnostics;
            Assert.Equal(1, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.Contains("Close } is missing in Expression", diagnostics[0].Message);

            diagnostics = Templates.ParseResource(new LGResource(string.Empty, string.Empty, "#Demo4\r\n- ${createArray(1,\r\n2,3)\r\n> this is a comment")).Diagnostics;
            Assert.Equal(1, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.Contains("Close } is missing in Expression", diagnostics[0].Message);
        }

        [Fact]
        public void TestErrorTemplateName()
        {
            var diagnostics = GetDiagnostics("ErrorTemplateName.lg");

            Assert.Equal(7, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.Contains(TemplateErrors.InvalidParameter("param1; param2"), diagnostics[0].Message);

            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.Contains(TemplateErrors.InvalidParameter("param1 param2"), diagnostics[1].Message);

            Assert.Equal(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.Contains(TemplateErrors.InvalidTemplateName("template3(errorparams"), diagnostics[2].Message);

            Assert.Equal(DiagnosticSeverity.Error, diagnostics[3].Severity);
            Assert.Contains(TemplateErrors.InvalidParameter("a)test(param1"), diagnostics[3].Message);

            Assert.Equal(DiagnosticSeverity.Error, diagnostics[4].Severity);
            Assert.Contains(TemplateErrors.InvalidParameter("$%^"), diagnostics[4].Message);

            Assert.Equal(DiagnosticSeverity.Error, diagnostics[5].Severity);
            Assert.Contains(TemplateErrors.InvalidTemplateName("the-name-of-template"), diagnostics[5].Message);

            Assert.Equal(DiagnosticSeverity.Error, diagnostics[6].Severity);
            Assert.Contains(TemplateErrors.InvalidTemplateName("t1.1"), diagnostics[6].Message);
        }

        [Fact]
        public void TestInvalidLGFileImportPath()
        {
            var diagnostics = GetDiagnostics("InvalidLGFileImportPath.lg");

            Assert.Equal(1, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.Contains("Could not find file", diagnostics[0].Message);
        }

        [Fact]
        public void TestLgTemplateFunctionError()
        {
            var diagnostics = GetDiagnostics("LgTemplateFunctionError.lg");

            Assert.Equal(2, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.Contains("Error occurred when parsing expression 'NotExistTemplate()'. NotExistTemplate does not have an evaluator", diagnostics[0].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.Contains("Error occurred when parsing expression 'template5('hello', 'world')'. arguments mismatch for template 'template5'. Expecting '1' arguments, actual '2'.", diagnostics[1].Message);
        }

        [Fact]
        public void TestMultiLineVariation()
        {
            var diagnostics = GetDiagnostics("MultiLineVariation.lg");

            Assert.Equal(1, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.Contains(TemplateErrors.NoEndingInMultiline, diagnostics[0].Message);
        }

        [Fact]
        public void TestNoNormalTemplateBody()
        {
            var diagnostics = GetDiagnostics("NoNormalTemplateBody.lg");

            Assert.Equal(3, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[0].Severity);
            Assert.Contains(TemplateErrors.NotEndWithElseInCondition, diagnostics[0].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.Contains(TemplateErrors.MissingTemplateBodyInCondition, diagnostics[1].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.Contains(TemplateErrors.MissingTemplateBodyInCondition, diagnostics[2].Message);
        }

        [Fact]
        public void TestNoTemplateRef()
        {
            var diagnostics = GetDiagnostics("NoTemplateRef.lg");

            Assert.Equal(3, diagnostics.Count);

            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.Contains("Error occurred when parsing expression 'templateRef()'. templateRef does not have an evaluator", diagnostics[0].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.Contains("Error occurred when parsing expression 'templateRef(a)'. templateRef does not have an evaluator", diagnostics[1].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.Contains("Error occurred when parsing expression 'templateRefInMultiLine()'. templateRefInMultiLine does not have an evaluator", diagnostics[2].Message);
        }

        [Fact]
        public void TestSwitchCaseFormatError()
        {
            var diagnostics = GetDiagnostics("SwitchCaseFormatError.lg");

            Assert.Equal(14, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.Contains(TemplateErrors.InvalidWhitespaceInSwitchCase, diagnostics[0].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.Contains(TemplateErrors.MultipleSwithStatementInSwitchCase, diagnostics[1].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.Contains(TemplateErrors.NotStartWithSwitchInSwitchCase, diagnostics[2].Message);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[3].Severity);
            Assert.Contains(TemplateErrors.MissingCaseInSwitchCase, diagnostics[3].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[4].Severity);
            Assert.Contains(TemplateErrors.InvalidStatementInMiddlerOfSwitchCase, diagnostics[4].Message);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[5].Severity);
            Assert.Contains(TemplateErrors.NotEndWithDefaultInSwitchCase, diagnostics[5].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[6].Severity);
            Assert.Contains(TemplateErrors.ExtraExpressionInSwitchCase, diagnostics[6].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[7].Severity);
            Assert.Contains(TemplateErrors.MissingTemplateBodyInSwitchCase, diagnostics[7].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[8].Severity);
            Assert.Contains(TemplateErrors.ExtraExpressionInSwitchCase, diagnostics[8].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[9].Severity);
            Assert.Contains(TemplateErrors.MissingTemplateBodyInSwitchCase, diagnostics[9].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[10].Severity);
            Assert.Contains(TemplateErrors.InvalidExpressionInSwiathCase, diagnostics[10].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[11].Severity);
            Assert.Contains(TemplateErrors.ExtraExpressionInSwitchCase, diagnostics[11].Message);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[12].Severity);
            Assert.Contains(TemplateErrors.MissingCaseInSwitchCase, diagnostics[12].Message);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[13].Severity);
            Assert.Contains(TemplateErrors.NotEndWithDefaultInSwitchCase, diagnostics[13].Message);
        }

        [Fact]
        public void TestLoopDetected()
        {
            var lgFile = GetTemplates("LoopDetected.lg");
            var exception = Assert.Throws<Exception>(() => lgFile.Evaluate("wPhrase"));
            Assert.Contains(TemplateErrors.LoopDetected, exception.Message);

            exception = Assert.Throws<Exception>(() => lgFile.AnalyzeTemplate("wPhrase"));
            Assert.Contains(TemplateErrors.LoopDetected, exception.Message);

            exception = Assert.Throws<Exception>(() => lgFile.AnalyzeTemplate("shouldFail"));
            Assert.Contains(TemplateErrors.LoopDetected, exception.Message);
        }

        [Fact]
        public void AddTextWithWrongId()
        {
            var diagnostics = Templates.ParseResource(new LGResource("a.lg", "a.lg", "[import](xx.lg) \r\n # t \n - hi")).Diagnostics;
            Assert.Equal(1, diagnostics.Count);
            Assert.Contains("Could not find file", diagnostics[0].Message);
        }

        [Fact]
        public void TestErrorExpression()
        {
            var lgFile = GetTemplates("ErrorExpression.lg");
            var exception = Assert.Throws<Exception>(() => lgFile.Evaluate("template1"));
            Assert.Contains("Error occurred when evaluating", exception.Message);
        }

        [Fact]
        public void TestRunTimeErrors()
        {
            var lgFile = GetTemplates("RunTimeErrors.lg");

            var exception = Assert.Throws<Exception>(() => lgFile.Evaluate("template1"));
            Assert.Equal("'dialog.abc' evaluated to null. [template1]  Error occurred when evaluating '-I want ${dialog.abc}'.", exception.Message);

            exception = Assert.Throws<Exception>(() => lgFile.Evaluate("prebuilt1"));
            Assert.Equal("'dialog.abc' evaluated to null. [prebuilt1]  Error occurred when evaluating '-I want ${foreach(dialog.abc, item, template1())}'.", exception.Message);

            exception = Assert.Throws<Exception>(() => lgFile.Evaluate("template2"));
            Assert.Equal("'dialog.abc' evaluated to null. [template1]  Error occurred when evaluating '-I want ${dialog.abc}'. [template2]  Error occurred when evaluating '-With composition ${template1()}'.", exception.Message);

            exception = Assert.Throws<Exception>(() => lgFile.Evaluate("conditionalTemplate1", new { dialog = true }));
            Assert.Equal("'dialog.abc' evaluated to null. [template1]  Error occurred when evaluating '-I want ${dialog.abc}'. [conditionalTemplate1] Condition '${dialog}':  Error occurred when evaluating '-I want ${template1()}'.", exception.Message);

            exception = Assert.Throws<Exception>(() => lgFile.Evaluate("conditionalTemplate2"));
            Assert.Equal("'dialog.abc' evaluated to null. [conditionalTemplate2] Condition '${dialog.abc}': Error occurred when evaluating '-IF :${dialog.abc}'.", exception.Message);

            exception = Assert.Throws<Exception>(() => lgFile.Evaluate("structured1"));
            Assert.Equal("'dialog.abc' evaluated to null. [structured1] Property 'Text': Error occurred when evaluating 'Text=I want ${dialog.abc}'.", exception.Message);

            exception = Assert.Throws<Exception>(() => lgFile.Evaluate("structured2"));
            Assert.Equal("'dialog.abc' evaluated to null. [template1]  Error occurred when evaluating '-I want ${dialog.abc}'. [structured2] Property 'Text': Error occurred when evaluating 'Text=I want ${template1()}'.", exception.Message);

            exception = Assert.Throws<Exception>(() => lgFile.Evaluate("structured3"));
            Assert.Equal("'dialog.abc' evaluated to null. [template1]  Error occurred when evaluating '-I want ${dialog.abc}'. [structured2] Property 'Text': Error occurred when evaluating 'Text=I want ${template1()}'. [structured3]  Error occurred when evaluating '${structured2()}'.", exception.Message);

            exception = Assert.Throws<Exception>(() => lgFile.Evaluate("switchcase1", new { turn = new { testValue = 1 } }));
            Assert.Equal("'dialog.abc' evaluated to null. [switchcase1] Case '${1}': Error occurred when evaluating '-I want ${dialog.abc}'.", exception.Message);

            exception = Assert.Throws<Exception>(() => lgFile.Evaluate("switchcase2", new { turn = new { testValue = 0 } }));
            Assert.Equal("'dialog.abc' evaluated to null. [switchcase2] Case 'Default': Error occurred when evaluating '-I want ${dialog.abc}'.", exception.Message);
        }

        [Fact]
        public void TestErrorLine()
        {
            var diagnostics = GetDiagnostics("ErrorLine.lg");

            Assert.Equal(4, diagnostics.Count);

            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.Contains(TemplateErrors.SyntaxError("mismatched input '-' expecting <EOF>"), diagnostics[0].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.Contains(TemplateErrors.InvalidStrucName("]"), diagnostics[1].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.Contains(TemplateErrors.MissingStrucEnd, diagnostics[2].Message);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[3].Severity);
            Assert.Contains(TemplateErrors.InvalidStrucBody("- hi"), diagnostics[3].Message);
        }

        [Fact]
        public void TestExpressionFormatError()
        {
            var diagnostics = GetDiagnostics("ExpressionFormatError.lg");
            Assert.Equal(1, diagnostics.Count);
            Assert.Contains("Close } is missing in Expression", diagnostics[0].Message);
        }

        [Fact]
        public void TestLoopReference()
        {
            var diagnostics = GetDiagnostics("CycleRef1.lg");
            Assert.Equal(1, diagnostics.Count);
            Assert.StartsWith(TemplateErrors.LoopDetected, diagnostics[0].Message);
        }

        private string GetExceptionExampleFilePath(string fileName)
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "ExceptionExamples" + Path.DirectorySeparatorChar + fileName;
        }

        private Templates GetTemplates(string fileName)
        {
            var filePath = GetExceptionExampleFilePath(fileName);
            return Templates.ParseFile(filePath);
        }

        private IList<Diagnostic> GetDiagnostics(string fileName)
        {
            var filePath = GetExceptionExampleFilePath(fileName);
            return Templates.ParseFile(filePath).Diagnostics;
        }
    }
}
