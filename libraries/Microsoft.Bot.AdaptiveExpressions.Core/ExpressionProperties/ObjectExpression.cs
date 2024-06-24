// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.Bot.AdaptiveExpressions.Core.Properties
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
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public ObjectExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        /// <param name="value">value.</param>
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public ObjectExpression(T value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        /// <param name="expressionOrString">expression or string.</param>
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public ObjectExpression(string expressionOrString)
            : base(expressionOrString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        /// <param name="expression">expression.</param>
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public ObjectExpression(Expression expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        /// <param name="lambda">function (data) which evaluates to object.</param>
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public ObjectExpression(Func<object, object> lambda)
            : this(Expression.Lambda(lambda))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        /// <param name="expressionOrValue">expression or value.</param>
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public ObjectExpression(JsonNode expressionOrValue)
            : base(expressionOrValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        /// <param name="typeInfo">typeInfo for serialization.</param>
        public ObjectExpression(JsonTypeInfo typeInfo)
            : base(typeInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        /// <param name="value">value.</param>
        /// <param name="typeInfo">typeInfo for serialization.</param>
        public ObjectExpression(T value, JsonTypeInfo typeInfo)
            : base(value, typeInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        /// <param name="expressionOrString">value.</param>
        /// <param name="typeInfo">typeInfo for serialization.</param>
        public ObjectExpression(string expressionOrString, JsonTypeInfo typeInfo)
            : base(expressionOrString, typeInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        /// <param name="lambda">value function.</param>
        /// <param name="typeInfo">typeInfo for serialization.</param>
        public ObjectExpression(Func<object, object> lambda, JsonTypeInfo typeInfo)
            : base(Expression.Lambda(lambda), typeInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectExpression{T}"/> class.
        /// </summary>
        /// <param name="node">value.</param>
        /// <param name="typeInfo">typeInfo for serialization.</param>
        public ObjectExpression(JsonNode node, JsonTypeInfo typeInfo)
            : base(node, typeInfo)
        {
        }

        /// <summary>
        /// Converts a value to an ObjectExpression instance.
        /// </summary>
        /// <param name="value">The value to convert.</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
        [RequiresUnreferencedCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        [RequiresDynamicCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        public static implicit operator ObjectExpression<T>(T value) => new ObjectExpression<T>(value);

        /// <summary>
        /// Converts a string value to an ObjectExpression instance.
        /// </summary>
        /// <param name="expressionOrString">A string value to convert.</param>
        [RequiresUnreferencedCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        [RequiresDynamicCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        public static implicit operator ObjectExpression<T>(string expressionOrString) => new ObjectExpression<T>(expressionOrString);

        /// <summary>
        /// Converts an Expression instance to an ObjectExpression instance.
        /// </summary>
        /// <param name="expression">The Expression instance to convert.</param>
        [RequiresUnreferencedCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        [RequiresDynamicCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        public static implicit operator ObjectExpression<T>(Expression expression) => new ObjectExpression<T>(expression);

        /// <summary>
        /// Converts a JSON Token to an ObjectExpression instance.
        /// </summary>
        /// <param name="expressionOrvalue">The JSON Token to convert.</param>
        [RequiresUnreferencedCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        [RequiresDynamicCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        public static implicit operator ObjectExpression<T>(JsonNode expressionOrvalue) => new ObjectExpression<T>(expressionOrvalue);
#pragma warning restore CA2225 // Operator overloads have named alternates
    }
}
