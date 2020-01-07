using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class LGFileDiagnosticTest
    {
        [TestMethod]
        public void TestConditionFormatError()
        {
            var diagnostics = GetDiagnostics("ConditionFormatError.lg");

            Assert.AreEqual(10, diagnostics.Count);
            Assert.AreEqual(DiagnosticSeverity.Warning, diagnostics[0].Severity);
            Assert.IsTrue(diagnostics[0].Message.Contains("condition is not end with else"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.IsTrue(diagnostics[1].Message.Contains("if and elseif should followed by one valid expression"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.IsTrue(diagnostics[2].Message.Contains("condition can't have more than one if"));
            Assert.AreEqual(DiagnosticSeverity.Warning, diagnostics[3].Severity);
            Assert.IsTrue(diagnostics[3].Message.Contains("condition is not end with else"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[4].Severity);
            Assert.IsTrue(diagnostics[4].Message.Contains("else should not followed by any expression"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[5].Severity);
            Assert.IsTrue(diagnostics[5].Message.Contains("condition can't have more than one if"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[6].Severity);
            Assert.IsTrue(diagnostics[6].Message.Contains("only elseif is allowed in middle of condition"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[7].Severity);
            Assert.IsTrue(diagnostics[7].Message.Contains("At most 1 whitespace is allowed between IF/ELSEIF/ELSE and :"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[8].Severity);
            Assert.IsTrue(diagnostics[8].Message.Contains("At most 1 whitespace is allowed between IF/ELSEIF/ELSE and :"));
            Assert.AreEqual(DiagnosticSeverity.Warning, diagnostics[9].Severity);
            Assert.IsTrue(diagnostics[9].Message.Contains("condition is not start with i"));
        }

        [TestMethod]
        public void TestDuplicatedTemplates()
        {
            var diagnostics = GetDiagnostics("DuplicatedTemplates.lg");

            Assert.AreEqual(2, diagnostics.Count);
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.IsTrue(diagnostics[0].Message.Contains("Duplicated definitions found for template: template1"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.IsTrue(diagnostics[1].Message.Contains("Duplicated definitions found for template: template1"));

            var lgFile = GetLGFile("DuplicatedTemplates.lg");
            var allDiagnostics = lgFile.AllDiagnostics;

            Assert.AreEqual(4, allDiagnostics.Count);
            Assert.AreEqual(DiagnosticSeverity.Error, allDiagnostics[0].Severity);
            Assert.IsTrue(allDiagnostics[0].Message.Contains("Duplicated definitions found for template: template1"));
            Assert.AreEqual(DiagnosticSeverity.Error, allDiagnostics[1].Severity);
            Assert.IsTrue(allDiagnostics[1].Message.Contains("Duplicated definitions found for template: template1"));
            Assert.AreEqual(DiagnosticSeverity.Error, allDiagnostics[2].Severity);
            Assert.IsTrue(allDiagnostics[2].Message.Contains("Duplicated definitions found for template: basicTemplate"));
            Assert.AreEqual(DiagnosticSeverity.Error, allDiagnostics[3].Severity);
            Assert.IsTrue(allDiagnostics[3].Message.Contains("Duplicated definitions found for template: basicTemplate2"));
        }

        [TestMethod]
        public void TestDuplicatedTemplatesInImportFiles()
        {
            var diagnostics = GetDiagnostics("DuplicatedTemplatesInImportFiles.lg");

            Assert.AreEqual(2, diagnostics.Count);
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.IsTrue(diagnostics[0].Message.Contains("Duplicated definitions found for template: basicTemplate"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.IsTrue(diagnostics[1].Message.Contains("Duplicated definitions found for template: basicTemplate2"));
        }

        [TestMethod]
        public void TestEmptyLGFile()
        {
            var diagnostics = GetDiagnostics("EmptyLGFile.lg");

            Assert.AreEqual(1, diagnostics.Count);
            Assert.AreEqual(DiagnosticSeverity.Warning, diagnostics[0].Severity);
            Assert.IsTrue(diagnostics[0].Message.Contains("File must have at least one template definition"));
        }

        [TestMethod]
        public void TestEmptyTemplate()
        {
            var diagnostics = GetDiagnostics("EmptyTemplate.lg");

            Assert.AreEqual(1, diagnostics.Count);
            Assert.AreEqual(DiagnosticSeverity.Warning, diagnostics[0].Severity);
            Assert.IsTrue(diagnostics[0].Message.Contains("There is no template body in template template"));
        }

        [TestMethod]
        public void TestErrorStructuredTemplate()
        {
            var diagnostics = GetDiagnostics("ErrorStructuredTemplate.lg");

            Assert.AreEqual(5, diagnostics.Count);
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.IsTrue(diagnostics[0].Message.Contains("structured body format error"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.IsTrue(diagnostics[1].Message.Contains("Structured content is empty"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.IsTrue(diagnostics[2].Message.Contains("does not have an evaluator"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[3].Severity);
            Assert.IsTrue(diagnostics[3].Message.Contains("does not have an evaluator"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[4].Severity);
            Assert.IsTrue(diagnostics[4].Message.Contains("structured name format error"));
        }

        [TestMethod]
        public void TestErrorTemplateName()
        {
            var diagnostics = GetDiagnostics("ErrorTemplateName.lg");

            Assert.AreEqual(5, diagnostics.Count);
            foreach (var diagnostic in diagnostics)
            {
                Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
                Assert.IsTrue(diagnostic.Message.Contains("Not a valid template name line"));
            }
        }

        [TestMethod]
        public void TestInvalidLGFileImportPath()
        {
            var diagnostics = GetDiagnostics("InvalidLGFileImportPath.lg");

            Assert.AreEqual(1, diagnostics.Count);
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.IsTrue(diagnostics[0].Message.Contains("Could not find file"));
        }

        [TestMethod]
        public void TestLgTemplateFunctionError()
        {
            var diagnostics = GetDiagnostics("LgTemplateFunctionError.lg");

            Assert.AreEqual(2, diagnostics.Count);
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.IsTrue(diagnostics[0].Message.Contains("does not have an evaluator"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.IsTrue(diagnostics[1].Message.Contains("arguments mismatch for template"));
        }

        [TestMethod]
        public void TestMultiLineVariation()
        {
            var diagnostics = GetDiagnostics("MultiLineVariation.lg");

            Assert.AreEqual(1, diagnostics.Count);
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.IsTrue(diagnostics[0].Message.Contains("Close ``` is missing"));
        }

        [TestMethod]
        public void TestNoNormalTemplateBody()
        {
            var diagnostics = GetDiagnostics("NoNormalTemplateBody.lg");

            Assert.AreEqual(3, diagnostics.Count);
            Assert.AreEqual(DiagnosticSeverity.Warning, diagnostics[0].Severity);
            Assert.IsTrue(diagnostics[0].Message.Contains("condition is not end with else"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.IsTrue(diagnostics[1].Message.Contains("no normal template body in condition block"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.IsTrue(diagnostics[2].Message.Contains("no normal template body in condition block"));
        }

        [TestMethod]
        public void TestNoTemplateRef()
        {
            var diagnostics = GetDiagnostics("NoTemplateRef.lg");

            Assert.AreEqual(3, diagnostics.Count);

            foreach (var diagnostic in diagnostics)
            {
                Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
                Assert.IsTrue(diagnostic.Message.Contains("does not have an evaluator"));
            }
        }

        [TestMethod]
        public void TestSwitchCaseFormatError()
        {
            var diagnostics = GetDiagnostics("SwitchCaseFormatError.lg");

            Assert.AreEqual(14, diagnostics.Count);
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[0].Severity);
            Assert.IsTrue(diagnostics[0].Message.Contains("At most 1 whitespace is allowed between SWITCH/CASE/DEFAULT and :."));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[1].Severity);
            Assert.IsTrue(diagnostics[1].Message.Contains("control flow can not have more than one switch statement"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[2].Severity);
            Assert.IsTrue(diagnostics[2].Message.Contains("control flow is not start with switch"));
            Assert.AreEqual(DiagnosticSeverity.Warning, diagnostics[3].Severity);
            Assert.IsTrue(diagnostics[3].Message.Contains("control flow should have at least one case statement"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[4].Severity);
            Assert.IsTrue(diagnostics[4].Message.Contains("only case statement is allowed in the middle of control flow"));
            Assert.AreEqual(DiagnosticSeverity.Warning, diagnostics[5].Severity);
            Assert.IsTrue(diagnostics[5].Message.Contains("control flow is not ending with default statement"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[6].Severity);
            Assert.IsTrue(diagnostics[6].Message.Contains("default should not followed by any expression or any text"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[7].Severity);
            Assert.IsTrue(diagnostics[7].Message.Contains("no normal template body in case or default block"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[8].Severity);
            Assert.IsTrue(diagnostics[8].Message.Contains("default should not followed by any expression or any text"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[9].Severity);
            Assert.IsTrue(diagnostics[9].Message.Contains("no normal template body in case or default block"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[10].Severity);
            Assert.IsTrue(diagnostics[10].Message.Contains("switch and case should followed by one valid expression"));
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[11].Severity);
            Assert.IsTrue(diagnostics[11].Message.Contains("default should not followed by any expression or any text"));
            Assert.AreEqual(DiagnosticSeverity.Warning, diagnostics[12].Severity);
            Assert.IsTrue(diagnostics[12].Message.Contains("control flow should have at least one case statement"));
            Assert.AreEqual(DiagnosticSeverity.Warning, diagnostics[13].Severity);
            Assert.IsTrue(diagnostics[13].Message.Contains("control flow is not ending with default statement"));
        }

        [TestMethod]
        public void TestLoopDetected()
        {
            var lgFile = GetLGFile("LoopDetected.lg");
            var exception = Assert.ThrowsException<Exception>(() => lgFile.EvaluateTemplate("wPhrase"));
            Assert.IsTrue(exception.Message.Contains("Loop detected"));

            exception = Assert.ThrowsException<Exception>(() => lgFile.AnalyzeTemplate("wPhrase"));
            Assert.IsTrue(exception.Message.Contains("Loop detected"));
        }

        [TestMethod]
        public void AddTextWithWrongId()
        {
            var diagnostics = LGParser.ParseText("[import](xx.lg) \r\n # t \n - hi", "a.lg").Diagnostics;
            Assert.AreEqual(1, diagnostics.Count);
            Assert.IsTrue(diagnostics[0].Message.Contains("Could not find file"));
        }

        [TestMethod]
        public void TestErrorExpression()
        {
            var lgFile = GetLGFile("ErrorExpression.lg");
            var exception = Assert.ThrowsException<Exception>(() => lgFile.EvaluateTemplate("template1"));
            Assert.IsTrue(exception.Message.Contains("Error occurs when evaluating expression"));
        }

        [TestMethod]
        public void TestNoVariableMatch()
        {
            var lgFile = GetLGFile("NoVariableMatch.lg");
            var exception = Assert.ThrowsException<Exception>(() => lgFile.EvaluateTemplate("NoVariableMatch"));
            Assert.IsTrue(exception.Message.Contains("Error occurs when evaluating expression 'Name': Name is evaluated to null"));
        }

        private string GetExceptionExampleFilePath(string fileName)
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "ExceptionExamples" + Path.DirectorySeparatorChar + fileName;
        }

        private LGFile GetLGFile(string fileName)
        {
            var filePath = GetExceptionExampleFilePath(fileName);
            return LGParser.ParseFile(filePath);
        }

        private IList<Diagnostic> GetDiagnostics(string fileName)
        {
            var filePath = GetExceptionExampleFilePath(fileName);
            return LGParser.ParseFile(filePath).Diagnostics;
        }
    }
}
