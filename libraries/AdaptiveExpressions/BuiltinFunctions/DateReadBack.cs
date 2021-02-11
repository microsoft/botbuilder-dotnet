// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Uses the date-time library to provide a date readback.
    /// </summary>
    internal class DateReadBack : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateReadBack"/> class.
        /// </summary>
        public DateReadBack()
            : base(ExpressionType.DateReadBack, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                args =>
                {
                    object result = null;
                    string error;
                    (result, error) = FunctionUtils.NormalizeToDateTime(args[0]);
                    if (error == null)
                    {
                        var timestamp1 = (DateTime)result;
                        (result, error) = FunctionUtils.NormalizeToDateTime(args[1]);
                        if (error == null)
                        {
                            var timestamp2 = (DateTime)result;
                            var timex = new TimexProperty(timestamp2.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                            result = TimexRelativeConvert.ConvertTimexToStringRelative(timex, timestamp1);
                        }
                    }

                    return (result, error);
                });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.String, ReturnType.String);
        }
    }
}
