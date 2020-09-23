// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Memory;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return true if a given TimexProperty or Timex expression refers to a valid time.
    /// Valid time contains hours, minutes and seconds.
    /// </summary>
    internal class TimexResolve : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimexResolve"/> class.
        /// </summary>
        public TimexResolve()
            : base(ExpressionType.TimexResolve, Evaluator, ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            TimexProperty parsed = null;
            string value = null;
            string error = null;
            IReadOnlyList<object> args;
            (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                (parsed, error) = FunctionUtils.ParseTimexProperty(args[0]);
            }

            // if the parsed TimexProperty has no types, then it cannot be resolved 
            if (error == null && parsed.Types.Count == 0)
            {
                error = $"The parsed TimexProperty of {args[0]} in {expression} has no types. It can't be resolved to a string value.";
            }

            if (error == null)
            {
                var formatedTimex = TimexFormat.Format(parsed);
                try
                {
                    var resolvedValues = TimexResolver.Resolve(new string[] { formatedTimex });
                    value = resolvedValues.Values[0].Value;
                } 
                catch (ArgumentException err)
                {
                    error = $"{args[0]} in {expression} is not a valid argument. {err.Message}";
                }
            }

            return (value, error);
        }
    }
}
