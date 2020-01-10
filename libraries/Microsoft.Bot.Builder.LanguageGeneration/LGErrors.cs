// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class LGErrors
    {
        public const string NoTemplate = "File must have at least one template definition";

        public const string InvalidTemplateName = "Not a valid template name line";

        public const string InvalidTemplateBody = "Invalid template body line, did you miss '-' at line begin";

        public const string InvalidStrucName = "structured name format error.";

        public const string MissingStrucEnd = "structured LG missing ending ']'";

        public const string EmptyStrucContent = "Structured content is empty";

        public const string InvalidStrucBody = "structured body format error.";

        public const string InvalidWhitespaceInCondition = "At most 1 whitespace is allowed between IF/ELSEIF/ELSE and :";

        public const string NotStartWithIfInCondition = "condition is not start with if";

        public const string MultipleIfInCondition = "condition can't have more than one if";

        public const string NotEndWithElseInCondition = "condition is not end with else";

        public const string InvalidMiddleInCondition = "only elseif is allowed in middle of condition";

        public const string InvalidExpressionInCondition = "if and elseif should followed by one valid expression";

        public const string ExtraExpressionInCondition = "else should not followed by any expression";

        public const string MissingTemplateBodyInCondition = "no normal template body in condition block";

        public const string InvalidWhitespaceInSwitchCase = "At most 1 whitespace is allowed between SWITCH/CASE/DEFAULT and :.";

        public const string NotStartWithSwitchInSwitchCase = "control flow is not start with switch";

        public const string MultipleSwithStatementInSwitchCase = "control flow can not have more than one switch statement";

        public const string InvalidStatementInMiddlerOfSwitchCase = "only case statement is allowed in the middle of control flow";

        public const string NotEndWithDefaultInSwitchCase = "control flow is not ending with default statement";

        public const string MissingCaseInSwitchCase = "control flow should have at least one case statement";

        public const string InvalidExpressionInSwiathCase = "switch and case should followed by one valid expression";

        public const string ExtraExpressionInSwitchCase = "default should not followed by any expression or any text";

        public const string MissingTemplateBodyInSwitchCase = "no normal template body in case or default block";

        public const string NoEndingInMultiline = "Close ``` is missing";

        public const string LoopDetected = "Loop detected:";

        public static string DuplicatedTemplateInSameTemplate(string templateName) => $"Duplicated definitions found for template: {templateName}";

        public static string DuplicatedTemplateInDiffTemplate(string templateName, string source) => $"Duplicated definitions found for template: {templateName} in {source}";

        public static string NoTemplateBody(string templateName) => $"There is no template body in template {templateName}";

        public static string TemplateNotExist(string templateName) => $"No such template {templateName}";

        public static string ErrorExpression(string expression, string error) => $"Error occurs when evaluating expression {expression}: {error}";

        public static string NullExpression(string expression) => $"Error occurs when evaluating expression '{expression}': {expression} is evaluated to null";

        public static string ArgumentMismatch(string templateName, int expectedCount, int actualCount) => $"arguments mismatch for template {templateName}, expect {expectedCount} actual {actualCount}";

        public static string ErrorTemplateNameformat(string templateName) => $"{templateName} can't be used as a template name, must be a string value";

        public static string TemplateExist(string templateName) => $"template {templateName} already exists.";
    }
}
