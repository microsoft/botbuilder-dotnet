// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the result from adding two or more numbers (pure number case) or concatting two or more strings (other case).
    /// </summary>
    internal class Add : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Add"/> class.
        /// </summary>
        public Add()
            : base(ExpressionType.Add, Evaluator(), ReturnType.String | ReturnType.Number, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplySequenceWithError(
                    args =>
                    {
                        object result = null;
                        string error = null;
                        var firstItem = args[0];
                        var secondItem = args[1];
                        var stringConcat = !firstItem.IsNumber() || !secondItem.IsNumber();

                        if ((firstItem == null && secondItem.IsNumber())
                            || (secondItem == null && firstItem.IsNumber()))
                        {
                            error = "Operator '+' or add cannot be applied to operands of type 'number' and null object.";
                        }
                        else
                        {
                            if (stringConcat)
                            {
                                result = $"{firstItem?.ToString()}{secondItem?.ToString()}";
                            }
                            else
                            {
                                result = EvalAdd(args[0], args[1]);
                            }
                        }

                        return (result, error);
                    }, FunctionUtils.VerifyNumberOrStringOrNull);
        }

        private static object EvalAdd(object a, object b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (a.IsInteger() && b.IsInteger())
            {
                return Convert.ToInt64(a, CultureInfo.InvariantCulture) + Convert.ToInt64(b, CultureInfo.InvariantCulture);
            }

            return FunctionUtils.CultureInvariantDoubleConvert(a) + FunctionUtils.CultureInvariantDoubleConvert(b);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 2, int.MaxValue, ReturnType.String | ReturnType.Number);
        }
    }
}
