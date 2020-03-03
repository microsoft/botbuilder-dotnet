// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.Properties
{
    /// <summary>
    /// ObjectExpression(T) - represents a property which is either an object of type T or a string expression which resolves to a object of type T.
    /// </summary>
    /// <typeparam name="T">the type of object.</typeparam>
    /// <remarks>String values are always interpreted as an expression, whether it has '=' prefix or not.</remarks>
    public class ObjectExpression<T> : ExpressionProperty<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        public ObjectExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        /// <param name="value">value.</param>
        public ObjectExpression(T value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        /// <param name="expressionOrString">expression or string.</param>
        public ObjectExpression(string expressionOrString)
            : base(expressionOrString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        /// <param name="expression">expression.</param>
        public ObjectExpression(Expression expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        /// <param name="lambda">function (data) which evaluates to object.</param>
        public ObjectExpression(Func<object, object> lambda)
            : this(Expression.Lambda(lambda))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        /// <param name="expressionOrValue">expression or value.</param>
        public ObjectExpression(JToken expressionOrValue)
            : base(expressionOrValue)
        {
        }

        public static implicit operator ObjectExpression<T>(T value) => new ObjectExpression<T>(value);

        public static implicit operator ObjectExpression<T>(string expressionOrString) => new ObjectExpression<T>(expressionOrString);

        public static implicit operator ObjectExpression<T>(Expression expression) => new ObjectExpression<T>(expression);

        public static implicit operator ObjectExpression<T>(JToken expressionOrvalue) => new ObjectExpression<T>(expressionOrvalue);
    }
}
