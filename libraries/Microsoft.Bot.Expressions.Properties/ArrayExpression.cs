// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Expressions.Properties
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
        public ArrayExpression(List<T> value)
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
        /// <param name="value">JToken which is either a collection of (T) or expression which evaluates to array.</param>
        public ArrayExpression(JToken value)
            : base(value)
        {
        }

        public static implicit operator ArrayExpression<T>(List<T> value) => new ArrayExpression<T>(value);

        public static implicit operator ArrayExpression<T>(string value) => new ArrayExpression<T>(value);

        public static implicit operator ArrayExpression<T>(JToken value) => new ArrayExpression<T>(value);
    }
}
