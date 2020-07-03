// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the ordinal number of the input number.
    /// </summary>
    public class AddOrdinal : ExpressionEvaluator
    {
        public AddOrdinal()
            : base(ExpressionType.AddOrdinal, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => EvalAddOrdinal(Convert.ToInt32(args[0])), FunctionUtils.VerifyInteger);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 1, 1, ReturnType.Number);
        }

        private static string EvalAddOrdinal(int num)
        {
            var hasResult = false;
            var ordinalResult = num.ToString();
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
