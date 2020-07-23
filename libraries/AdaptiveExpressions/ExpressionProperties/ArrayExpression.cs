// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.Properties
{
    /// <summary>
    /// ArrayExpression - represents a property which is either a value of array of T or a string expression to bind to a array of T.
    /// </summary>
    /// <typeparam name="T">type of object in the array.</typeparam>
    /// <remarks>String values are always interpreted as an expression, whether it has '=' prefix or not.</remarks>
    public class ArrayExpression<T> : ExpressionProperty<List<T>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        public ArrayExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        /// <param name="value">collection of (T).</param>
        public ArrayExpression(IEnumerable<T> value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        /// <param name="expression">expression which evaluates to array.</param>
        public ArrayExpression(string expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        /// <param name="expression">expression which evaluates to array.</param>
        public ArrayExpression(Expression expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        /// <param name="lambda">function (data) which evaluates to array.</param>
        public ArrayExpression(Func<object, object> lambda)
            : this(Expression.Lambda(lambda))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        /// <param name="expressionOrValue">JToken which is either a collection of (T) or expression which evaluates to array.</param>
        public ArrayExpression(JToken expressionOrValue)
            : base(expressionOrValue)
        {
        }

        /// <summary>
        /// Converts an array to ArrayExpression.
        /// </summary>
        /// <param name="value">The array to convert.</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static implicit operator ArrayExpression<T>(T[] value) => new ArrayExpression<T>(value);

        /// <summary>
        /// Converts a list to ArrayExpression.
        /// </summary>
        /// <param name="value">The list to convert.</param>
        public static implicit operator ArrayExpression<T>(List<T> value) => new ArrayExpression<T>(value);

        /// <summary>
        /// Converts a string to ArrayExpression.
        /// </summary>
        /// <param name="expression">The string to convert.</param>
        public static implicit operator ArrayExpression<T>(string expression) => new ArrayExpression<T>(expression);

        /// <summary>
        /// Converts an Expression instance to ArrayExpression.
        /// </summary>
        /// <param name="expression">The Expression instance to convert.</param>
        public static implicit operator ArrayExpression<T>(Expression expression) => new ArrayExpression<T>(expression);

        /// <summary>
        /// Converts a JSON Token to ArrayExpression.
        /// </summary>
        /// <param name="expressionOrValue">The JSON Token to Convert.</param>
        public static implicit operator ArrayExpression<T>(JToken expressionOrValue) => new ArrayExpression<T>(expressionOrValue);
#pragma warning restore CA2225 // Operator overloads have named alternates
    }
}
