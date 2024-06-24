// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.Bot.AdaptiveExpressions.Core.Properties
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
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public ArrayExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        /// <param name="value">collection of (T).</param>
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public ArrayExpression(IEnumerable<T> value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        /// <param name="expression">expression which evaluates to array.</param>
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public ArrayExpression(string expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        /// <param name="expression">expression which evaluates to array.</param>
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public ArrayExpression(Expression expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        /// <param name="lambda">function (data) which evaluates to array.</param>
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public ArrayExpression(Func<object, object> lambda)
            : this(Expression.Lambda(lambda))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        /// <param name="expressionOrValue">JsonNode which is either a collection of (T) or expression which evaluates to array.</param>
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public ArrayExpression(JsonNode expressionOrValue)
            : base(expressionOrValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        /// <param name="typeInfo">typeInfo.</param>
        public ArrayExpression(JsonTypeInfo typeInfo)
            : base(typeInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        /// <param name="value">collection of (T).</param>
        /// <param name="typeInfo">typeInfo.</param>
        public ArrayExpression(List<T> value, JsonTypeInfo typeInfo)
            : base(value, typeInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        /// <param name="value">collection of (T).</param>
        /// <param name="typeInfo">typeInfo.</param>
        public ArrayExpression(Expression value, JsonTypeInfo typeInfo)
            : base(value, typeInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        /// <param name="value">collection of (T).</param>
        /// <param name="typeInfo">typeInfo.</param>
        public ArrayExpression(string value, JsonTypeInfo typeInfo)
            : base(value, typeInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayExpression{T}"/> class.
        /// </summary>
        /// <param name="value">collection of (T).</param>
        /// <param name="typeInfo">typeInfo.</param>
        public ArrayExpression(JsonNode value, JsonTypeInfo typeInfo)
            : base(value, typeInfo)
        {
        }

        /// <summary>
        /// Converts an array to ArrayExpression.
        /// </summary>
        /// <param name="value">The array to convert.</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
        [RequiresUnreferencedCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        [RequiresDynamicCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        public static implicit operator ArrayExpression<T>(T[] value) => new ArrayExpression<T>(value);

        /// <summary>
        /// Converts a list to ArrayExpression.
        /// </summary>
        /// <param name="value">The list to convert.</param>
        [RequiresUnreferencedCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        [RequiresDynamicCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        public static implicit operator ArrayExpression<T>(List<T> value) => new ArrayExpression<T>(value);

        /// <summary>
        /// Converts a string to ArrayExpression.
        /// </summary>
        /// <param name="expression">The string to convert.</param>
        [RequiresUnreferencedCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        [RequiresDynamicCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        public static implicit operator ArrayExpression<T>(string expression) => new ArrayExpression<T>(expression);

        /// <summary>
        /// Converts an Expression instance to ArrayExpression.
        /// </summary>
        /// <param name="expression">The Expression instance to convert.</param>
        [RequiresUnreferencedCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        [RequiresDynamicCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        public static implicit operator ArrayExpression<T>(Expression expression) => new ArrayExpression<T>(expression);

        /// <summary>
        /// Converts a JSON Token to ArrayExpression.
        /// </summary>
        /// <param name="expressionOrValue">The JSON Token to Convert.</param>
        [RequiresUnreferencedCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        [RequiresDynamicCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        public static implicit operator ArrayExpression<T>(JsonNode expressionOrValue) => new ArrayExpression<T>(expressionOrValue);
#pragma warning restore CA2225 // Operator overloads have named alternates
    }
}
