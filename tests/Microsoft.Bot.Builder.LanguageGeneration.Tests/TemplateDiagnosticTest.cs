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
            Assert.True(diagnostics[0].Message.Contains(TemplateErrors.NotEndWithElseInCondition));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.True(diagnostics[1].Message.Contains(TemplateErrors.InvalidExpressionInCondition));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.True(diagnostics[2].Message.Contains(TemplateErrors.MultipleIfInCondition));
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[3].Severity);
            Assert.True(diagnostics[3].Message.Contains(TemplateErrors.NotEndWithElseInCondition));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[4].Severity);
            Assert.True(diagnostics[4].Message.Contains(TemplateErrors.ExtraExpressionInCondition));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[5].Severity);
            Assert.True(diagnostics[5].Message.Contains(TemplateErrors.MultipleIfInCondition));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[6].Severity);
            Assert.True(diagnostics[6].Message.Contains(TemplateErrors.InvalidMiddleInCondition));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[7].Severity);
            Assert.True(diagnostics[7].Message.Contains(TemplateErrors.InvalidWhitespaceInCondition));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[8].Severity);
            Assert.True(diagnostics[8].Message.Contains(TemplateErrors.InvalidWhitespaceInCondition));
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[9].Severity);
            Assert.True(diagnostics[9].Message.Contains(TemplateErrors.NotStartWithIfInCondition));
        }

        [Fact]
        public void TestDuplicatedTemplates()
        {
            var diagnostics = GetDiagnostics("DuplicatedTemplates.lg");

            Assert.Equal(2, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.True(diagnostics[0].Message.Contains(TemplateErrors.DuplicatedTemplateInSameTemplate("template1")));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.True(diagnostics[1].Message.Contains(TemplateErrors.DuplicatedTemplateInSameTemplate("template1")));

            var lgFile = GetTemplates("DuplicatedTemplates.lg");
            var allDiagnostics = lgFile.AllDiagnostics;

            Assert.Equal(4, allDiagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, allDiagnostics[0].Severity);
            Assert.True(allDiagnostics[0].Message.Contains(TemplateErrors.DuplicatedTemplateInSameTemplate("template1")));
            Assert.Equal(DiagnosticSeverity.Error, allDiagnostics[1].Severity);
            Assert.True(allDiagnostics[1].Message.Contains(TemplateErrors.DuplicatedTemplateInSameTemplate("template1")));
            Assert.Equal(DiagnosticSeverity.Error, allDiagnostics[2].Severity);
            Assert.True(allDiagnostics[2].Message.Contains("Duplicated definitions found for template: 'basicTemplate'"));
            Assert.Equal(DiagnosticSeverity.Error, allDiagnostics[3].Severity);
            Assert.True(allDiagnostics[3].Message.Contains("Duplicated definitions found for template: 'basicTemplate2'"));
        }

        [Fact]
        public void TestDuplicatedTemplatesInImportFiles()
        {
            var diagnostics = GetDiagnostics("DuplicatedTemplatesInImportFiles.lg");

            Assert.Equal(2, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.True(diagnostics[0].Message.Contains("Duplicated definitions found for template: 'basicTemplate'"));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.True(diagnostics[1].Message.Contains("Duplicated definitions found for template: 'basicTemplate2'"));
        }

        [Fact]
        public void TestEmptyLGFile()
        {
            var diagnostics = GetDiagnostics("EmptyLGFile.lg");

            Assert.Equal(1, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[0].Severity);
            Assert.True(diagnostics[0].Message.Contains(TemplateErrors.NoTemplate));
        }

        [Fact]
        public void TestEmptyTemplate()
        {
            var diagnostics = GetDiagnostics("EmptyTemplate.lg");

            Assert.Equal(1, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[0].Severity);
            Assert.True(diagnostics[0].Message.Contains(TemplateErrors.NoTemplateBody("template")));
        }

        [Fact]
        public void TestErrorStructuredTemplate()
        {
            var diagnostics = GetDiagnostics("ErrorStructuredTemplate.lg");

            Assert.Equal(8, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.True(diagnostics[0].Message.Contains(TemplateErrors.InvalidStrucBody("abc")));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.True(diagnostics[1].Message.Contains(TemplateErrors.EmptyStrucContent));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.True(diagnostics[2].Message.Contains("Error occurred when parsing expression 'NOTemplate()'. NOTemplate does not have an evaluator"));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[3].Severity);
            Assert.True(diagnostics[3].Message.Contains("Error occurred when parsing expression 'NOTemplate()'. NOTemplate does not have an evaluator"));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[4].Severity);
            Assert.True(diagnostics[4].Message.Contains(TemplateErrors.InvalidStrucName("Activity%")));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[5].Severity);
            Assert.True(diagnostics[5].Message.Contains(TemplateErrors.InvalidStrucName("Activity]")));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[6].Severity); 
            Assert.True(diagnostics[6].Message.Contains(TemplateErrors.MissingStrucEnd));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[7].Severity);
            Assert.True(diagnostics[7].Message.Contains(TemplateErrors.InvalidStrucBody("- hi")));
        }

        [Fact]
        public void TestErrorMultiLineExpr()
        {
            var diagnostics = GetDiagnostics("MultiLineExprError.lg");

            Assert.Equal(1, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.Contains("Close } is missing in Expression", diagnostics[0].Message);

            diagnostics = Templates.ParseText("#Demo2\r\n- ${createArray(1,\r\n, 2,3)").Diagnostics;
            Assert.Equal(1, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.Contains("Close } is missing in Expression", diagnostics[0].Message);

            diagnostics = Templates.ParseText("#Demo4\r\n- ${createArray(1,\r\n2,3)\r\n> this is a comment").Diagnostics;
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
            Assert.True(diagnostics[0].Message.Contains(TemplateErrors.InvalidParameter("param1; param2")));

            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.True(diagnostics[1].Message.Contains(TemplateErrors.InvalidParameter("param1 param2")));

            Assert.Equal(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.True(diagnostics[2].Message.Contains(TemplateErrors.InvalidTemplateName("template3(errorparams")));

            Assert.Equal(DiagnosticSeverity.Error, diagnostics[3].Severity);
            Assert.True(diagnostics[3].Message.Contains(TemplateErrors.InvalidParameter("a)test(param1")));

            Assert.Equal(DiagnosticSeverity.Error, diagnostics[4].Severity);
            Assert.True(diagnostics[4].Message.Contains(TemplateErrors.InvalidParameter("$%^")));

            Assert.Equal(DiagnosticSeverity.Error, diagnostics[5].Severity);
            Assert.True(diagnostics[5].Message.Contains(TemplateErrors.InvalidTemplateName("the-name-of-template")));

            Assert.Equal(DiagnosticSeverity.Error, diagnostics[6].Severity);
            Assert.True(diagnostics[6].Message.Contains(TemplateErrors.InvalidTemplateName("t1.1")));
        }

        [Fact]
        public void TestInvalidLGFileImportPath()
        {
            var diagnostics = GetDiagnostics("InvalidLGFileImportPath.lg");

            Assert.Equal(1, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.True(diagnostics[0].Message.Contains("Could not find file"));
        }

        [Fact]
        public void TestLgTemplateFunctionError()
        {
            var diagnostics = GetDiagnostics("LgTemplateFunctionError.lg");

            Assert.Equal(2, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.True(diagnostics[0].Message.Contains("Error occurred when parsing expression 'NotExistTemplate()'. NotExistTemplate does not have an evaluator"));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.True(diagnostics[1].Message.Contains("Error occurred when parsing expression 'template5('hello', 'world')'. arguments mismatch for template 'template5'. Expecting '1' arguments, actual '2'."));
        }

        [Fact]
        public void TestMultiLineVariation()
        {
            var diagnostics = GetDiagnostics("MultiLineVariation.lg");

            Assert.Equal(1, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.True(diagnostics[0].Message.Contains(TemplateErrors.NoEndingInMultiline));
        }

        [Fact]
        public void TestNoNormalTemplateBody()
        {
            var diagnostics = GetDiagnostics("NoNormalTemplateBody.lg");

            Assert.Equal(3, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[0].Severity);
            Assert.True(diagnostics[0].Message.Contains(TemplateErrors.NotEndWithElseInCondition));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.True(diagnostics[1].Message.Contains(TemplateErrors.MissingTemplateBodyInCondition));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.True(diagnostics[2].Message.Contains(TemplateErrors.MissingTemplateBodyInCondition));
        }

        [Fact]
        public void TestNoTemplateRef()
        {
            var diagnostics = GetDiagnostics("NoTemplateRef.lg");

            Assert.Equal(3, diagnostics.Count);

            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.True(diagnostics[0].Message.Contains("Error occurred when parsing expression 'templateRef()'. templateRef does not have an evaluator"));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.True(diagnostics[1].Message.Contains("Error occurred when parsing expression 'templateRef(a)'. templateRef does not have an evaluator"));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.True(diagnostics[2].Message.Contains("Error occurred when parsing expression 'templateRefInMultiLine()'. templateRefInMultiLine does not have an evaluator"));
        }

        [Fact]
        public void TestSwitchCaseFormatError()
        {
            var diagnostics = GetDiagnostics("SwitchCaseFormatError.lg");

            Assert.Equal(14, diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.True(diagnostics[0].Message.Contains(TemplateErrors.InvalidWhitespaceInSwitchCase));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.True(diagnostics[1].Message.Contains(TemplateErrors.MultipleSwithStatementInSwitchCase));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.True(diagnostics[2].Message.Contains(TemplateErrors.NotStartWithSwitchInSwitchCase));
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[3].Severity);
            Assert.True(diagnostics[3].Message.Contains(TemplateErrors.MissingCaseInSwitchCase));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[4].Severity);
            Assert.True(diagnostics[4].Message.Contains(TemplateErrors.InvalidStatementInMiddlerOfSwitchCase));
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[5].Severity);
            Assert.True(diagnostics[5].Message.Contains(TemplateErrors.NotEndWithDefaultInSwitchCase));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[6].Severity);
            Assert.True(diagnostics[6].Message.Contains(TemplateErrors.ExtraExpressionInSwitchCase));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[7].Severity);
            Assert.True(diagnostics[7].Message.Contains(TemplateErrors.MissingTemplateBodyInSwitchCase));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[8].Severity);
            Assert.True(diagnostics[8].Message.Contains(TemplateErrors.ExtraExpressionInSwitchCase));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[9].Severity);
            Assert.True(diagnostics[9].Message.Contains(TemplateErrors.MissingTemplateBodyInSwitchCase));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[10].Severity);
            Assert.True(diagnostics[10].Message.Contains(TemplateErrors.InvalidExpressionInSwiathCase));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[11].Severity);
            Assert.True(diagnostics[11].Message.Contains(TemplateErrors.ExtraExpressionInSwitchCase));
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[12].Severity);
            Assert.True(diagnostics[12].Message.Contains(TemplateErrors.MissingCaseInSwitchCase));
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[13].Severity);
            Assert.True(diagnostics[13].Message.Contains(TemplateErrors.NotEndWithDefaultInSwitchCase));
        }

        [Fact]
        public void TestLoopDetected()
        {
            var lgFile = GetTemplates("LoopDetected.lg");
            var exception = Assert.Throws<Exception>(() => lgFile.Evaluate("wPhrase"));
            Assert.True(exception.Message.Contains(TemplateErrors.LoopDetected));

            exception = Assert.Throws<Exception>(() => lgFile.AnalyzeTemplate("wPhrase"));
            Assert.True(exception.Message.Contains(TemplateErrors.LoopDetected));

            exception = Assert.Throws<Exception>(() => lgFile.AnalyzeTemplate("shouldFail"));
            Assert.True(exception.Message.Contains(TemplateErrors.LoopDetected));
        }

        [Fact]
        public void AddTextWithWrongId()
        {
            var diagnostics = Templates.ParseText("[import](xx.lg) \r\n # t \n - hi", "a.lg").Diagnostics;
            Assert.Equal(1, diagnostics.Count);
            Assert.True(diagnostics[0].Message.Contains("Could not find file"));
        }

        [Fact]
        public void TestErrorExpression()
        {
            var lgFile = GetTemplates("ErrorExpression.lg");
            var exception = Assert.Throws<Exception>(() => lgFile.Evaluate("template1"));
            Assert.True(exception.Message.Contains("Error occurred when evaluating"));
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
            Assert.True(diagnostics[0].Message.Contains(TemplateErrors.SyntaxError("mismatched input '-' expecting <EOF>")));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.True(diagnostics[1].Message.Contains(TemplateErrors.InvalidStrucName("]")));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.True(diagnostics[2].Message.Contains(TemplateErrors.MissingStrucEnd));
            Assert.Equal(DiagnosticSeverity.Error, diagnostics[3].Severity);
            Assert.True(diagnostics[3].Message.Contains(TemplateErrors.InvalidStrucBody("- hi")));
        }

        [Fact]
        public void TestExpressionFormatError()
        {
            var diagnostics = GetDiagnostics("ExpressionFormatError.lg");
            Assert.Equal(1, diagnostics.Count);
            Assert.True(diagnostics[0].Message.Contains("Close } is missing in Expression"));
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
