// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Expressions
{
    /// <summary>
    /// Constant expression.
    /// </summary>
    public class Constant : Expression
    {
        private object _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Constant"/> class.
        /// Construct an expression constant.
        /// </summary>
        /// <param name="value">Constant value.</param>
        public Constant(object value = null)
            : base(new ExpressionEvaluator(ExpressionType.Constant, (expression, state) => ((expression as Constant).Value, null)))
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
                      : ReturnType.Object;
                _value = value;
            }
        }

        public override string ToString()
        {
            if (Value == null)
            {
                return "null";
            }
            else if (Value is string)
            {
                return $"'{Value}'";
            }
            else
            {
                return Value?.ToString();
            }
        }
    }
}
