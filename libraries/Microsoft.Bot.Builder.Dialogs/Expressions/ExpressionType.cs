using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Expressions
{
    public class ExpressionType
    {
        // Math
        public const string Add = "+";
        public const string Subtract = "-";
        public const string Multiply = "*";

        // Comparisons
        public const string LessThan = "<";
        public const string LessThanOrEqual = "<=";
        public new const string Equals = "==";
        public const string NotEquals = "!=";
        public const string GreaterThan = ">";
        public const string GreaterThanOrEqual = ">=";

        // Logic
        public const string And = "&&";
        public const string Or = "||";
        public const string Not = "!";

        // Misc
        public const string Call = "Call";
        public const string Constant = "Constant";
        public const string Variable = "Variable";
    }
}
