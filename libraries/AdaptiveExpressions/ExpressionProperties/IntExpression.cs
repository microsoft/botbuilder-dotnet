// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.Properties
{
    /// <summary>
    /// IntExpression - represents a property which is either an Integer or a string expression which resolves to a Integer.
    /// </summary>
    /// <remarks>String values are always interpreted as an expression, whether it has '=' prefix or not.</remarks>
    [JsonConverter(typeof(IntExpressionConverter))]
    public class IntExpression : ExpressionProperty<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntExpression"/> class.
        /// </summary>
        public IntExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntExpression"/> class.
        /// </summary>
        /// <param name="value">value to return.</param>
        public IntExpression(int value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntExpression"/> class.
        /// </summary>
        /// <param name="expression">string expression to resolve to an int.</param>
        public IntExpression(string expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntExpression"/> class.
        /// </summary>
        /// <param name="expression">expression to resolve to an int.</param>
        public IntExpression(Expression expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntExpression"/> class.
        /// </summary>
        /// <param name="lambda">function (data) which evaluates to int.</param>
        public IntExpression(Func<object, object> lambda)
            : this(Expression.Lambda(lambda))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntExpression"/> class.
        /// </summary>
        /// <param name="expressionOrValue">JToken to resolve to an int.</param>
        public IntExpression(JToken expressionOrValue)
            : base(expressionOrValue)
        {
        }

        public static implicit operator IntExpression(int value) => new IntExpression(value);

        public static implicit operator IntExpression(string expression) => new IntExpression(expression);

        public static implicit operator IntExpression(Expression expression) => new IntExpression(expression);

        public static implicit operator IntExpression(JToken expressionOrValue) => new IntExpression(expressionOrValue);
    }
}
