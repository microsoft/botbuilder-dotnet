// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Expressions
{
    // NOTE: If you add a built-in type here, you also need to make sure it can be evaluated as well either by adding to BuiltInFunctions or when you construct the expression.
    /// <summary>
    /// Built-in expression types.
    /// </summary>
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
        public const string Equal = "==";
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
        public const string Constant = "Constant";
        public const string Lambda = "Lambda";
    }
}
