// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the ordinal number of the input number.
    /// </summary>
    internal class AddOrdinal : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddOrdinal"/> class.
        /// </summary>
        public AddOrdinal()
            : base(ExpressionType.AddOrdinal, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                args => 
                {
                    object result = null;
                    string error = null;
                    var input = 0;
                    (input, error) = FunctionUtils.ParseInt32(args[0]);
                    if (error == null)
                    {
                        result = EvalAddOrdinal(input);
                    }

                    return (result, error);
                }, FunctionUtils.VerifyInteger);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 1, 1, ReturnType.Number);
        }

        private static string EvalAddOrdinal(int num)
        {
            var hasResult = false;
            var ordinalResult = num.ToString(CultureInfo.InvariantCulture);
            if (num > 0)
            {
                switch (num % 100)
                {
                    case 11:
                    case 12:
                    case 13:
                        ordinalResult += "th";
                        hasResult = true;
                        break;
                }

                if (!hasResult)
                {
                    switch (num % 10)
                    {
                        case 1:
                            ordinalResult += "st";
                            break;
                        case 2:
                            ordinalResult += "nd";
                            break;
                        case 3:
                            ordinalResult += "rd";
                            break;
                        default:
                            ordinalResult += "th";
                            break;
                    }
                }
            }

            return ordinalResult;
        }
    }
}
