// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.Properties
{
    /// <summary>
    /// NumberExpression - represents a property which is either a float or a string expression which resolves to a float.
    /// </summary>
    /// <remarks>String values are always interpreted as an expression, whether it has '=' prefix or not.</remarks>
    [JsonConverter(typeof(NumberExpressionConverter))]
    public class NumberExpression : ExpressionProperty<float>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        public NumberExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        /// <param name="value">value to use.</param>
        public NumberExpression(float value) 
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        /// <param name="expression">string to interpret as expression or number.</param>
        public NumberExpression(string expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        /// <param name="expression">expression.</param>
        public NumberExpression(Expression expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        /// <param name="lambda">expression.</param>
        public NumberExpression(Func<object, object> lambda)
            : this(Expression.Lambda(lambda))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        /// <param name="expressionOrValue">jtoken to interpret as expression or number.</param>
        public NumberExpression(JToken expressionOrValue)
            : base(expressionOrValue)
        {
        }

        public static implicit operator NumberExpression(float value) => new NumberExpression(value);
        
        public static implicit operator NumberExpression(string expression) => new NumberExpression(expression);
        
        public static implicit operator NumberExpression(Expression expression) => new NumberExpression(expression);

        public static implicit operator NumberExpression(JToken expressionOrValue) => new NumberExpression(expressionOrValue);
    }
}
