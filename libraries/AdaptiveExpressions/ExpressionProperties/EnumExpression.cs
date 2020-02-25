// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.Properties
{
    /// <summary>
    /// EnumExpression - represents a property which is either a enum(T) or a string expression which resolves to a enum(T).
    /// </summary>
    /// <typeparam name="T">type of enum.</typeparam>
    /// <remarks>String values are always interpreted as an enum, unless it has '=' prefix in which case it is evaluated as a expression.</remarks>
    public class EnumExpression<T> : ExpressionProperty<T>
        where T : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpression{T}"/> class.
        /// </summary>
        public EnumExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpression{T}"/> class.
        /// </summary>
        /// <param name="value">value of T.</param>
        public EnumExpression(T value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpression{T}"/> class.
        /// </summary>
        /// <param name="expression">expression to resolve to an enum.</param>
        public EnumExpression(string expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpression{T}"/> class.
        /// </summary>
        /// <param name="expression">expression to resolve to an enum.</param>
        public EnumExpression(Expression expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpression{T}"/> class.
        /// </summary>
        /// <param name="lambda">function (data) which evaluates to enum.</param>
        public EnumExpression(Func<object, object> lambda)
            : this(Expression.Lambda(lambda))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpression{T}"/> class.
        /// </summary>
        /// <param name="expressionOrValue">jtoken value to resolve to an enum.</param>
        public EnumExpression(JToken expressionOrValue)
            : base(expressionOrValue)
        {
        }

        public static implicit operator EnumExpression<T>(T value) => new EnumExpression<T>(value);

        public static implicit operator EnumExpression<T>(string expressionOrValue) => new EnumExpression<T>(expressionOrValue);

        public static implicit operator EnumExpression<T>(Expression expression) => new EnumExpression<T>(expression);

        public static implicit operator EnumExpression<T>(JToken value) => new EnumExpression<T>(value);

        public override void SetValue(object value)
        {
            if (value is string stringOrExpression)
            {
                // if the expression is the enum value, then use that as the value, else it is an expression.
                if (Enum.TryParse<T>(stringOrExpression.TrimStart('='), ignoreCase: true, out T val))
                {
                    this.Value = val;
                    return;
                }
            }

            base.SetValue(value);
        }
    }
}
