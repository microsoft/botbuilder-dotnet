using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Expressions
{
    // NOTE: If you add a built-in type here, you also need to make sure it can be evaluated as well.
    public class ExpressionType
    {
        // Math
        public const string Add = "+";
        public const string Subtract = "-";
        public const string Multiply = "*";
        public const string Divide = "/";
        public const string Min = "min";
        public const string Max = "max";

        // Comparisons
        public const string LessThan = "<";
        public const string LessThanOrEqual = "<=";
        public new const string Equal = "==";
        public const string NotEqual = "!=";
        public const string GreaterThan = ">";
        public const string GreaterThanOrEqual = ">=";

        // Logic
        public const string And = "&&";
        public const string Or = "||";
        public const string Not = "!";

        // Memory
        public const string Accessor = "Accessor";
        public const string Element = "Element";

        // Misc
        public const string Call = "Call";
        public const string Constant = "Constant";
    }
}
