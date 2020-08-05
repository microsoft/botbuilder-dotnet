// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AdaptiveExpressions
{
    /// <summary>
    /// Constant expression.
    /// </summary>
    public class Constant : Expression
    {
        private readonly Regex _singleQuotRegex = new Regex(@"(?<!\\)'");
        private object _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Constant"/> class.
        /// Construct an expression constant.
        /// </summary>
        /// <param name="value">Constant value.</param>
        public Constant(object value = null)
            : base(new ExpressionEvaluator(ExpressionType.Constant, (expression, state, _) => ((expression as Constant).Value, null)))
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets constant value.
        /// </summary>
        /// <value>
        /// Constant value.
        /// </value>
        public object Value
        {
            get
            {
                return _value;
            }

            set
            {
                Evaluator.ReturnType =
                      value is string ? ReturnType.String
                      : value.IsNumber() ? ReturnType.Number
                      : value is bool ? ReturnType.Boolean
                      : FunctionUtils.TryParseList(value, out _) ? ReturnType.Array
                      : ReturnType.Object;
                _value = value;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string value.</returns>
        public override string ToString()
        {
            if (Value == null)
            {
                return "null";
            }
            else if (Value is string value)
            {
                var result = value.Replace(@"\", @"\\");

                result = _singleQuotRegex.Replace(result, new MatchEvaluator(m =>
                {
                    var value = m.Value;

                    // ' -> \'
                    return @"\'";
                }));

                return $"'{result}'";
            }
            else if (Value is float || Value is double)
            {
               return ((double)Value).ToString("0.00########", CultureInfo.InvariantCulture);
            }
            else
            {
                return Value?.ToString();
            }
        }

        /// <summary>
        /// Determines if the current Expression instance is deep equal to another one.
        /// </summary>
        /// <param name="other">The other Expression instance to compare.</param>
        /// <returns>A boolean value indicating  whether the two Expressions are deep equal. Reyurns True if they are deep equal, otherwise return False.</returns>
        public override bool DeepEquals(Expression other)
        {
            bool eq;
            if (other == null || other.Type != ExpressionType.Constant)
            {
                eq = false;
            }
            else
            {
                var otherVal = ((Constant)other).Value;
                eq = Value == otherVal || (Value != null && Value.Equals(otherVal));
            }

            return eq;
        }
    }
}
