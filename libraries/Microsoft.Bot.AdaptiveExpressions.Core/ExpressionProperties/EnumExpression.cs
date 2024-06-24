// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Bot.AdaptiveExpressions.Core.Converters;

namespace Microsoft.Bot.AdaptiveExpressions.Core.Properties
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
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public EnumExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpression{T}"/> class.
        /// </summary>
        /// <param name="value">value of T.</param>
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public EnumExpression(T value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpression{T}"/> class.
        /// </summary>
        /// <param name="expression">expression to resolve to an enum.</param>
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public EnumExpression(string expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpression{T}"/> class.
        /// </summary>
        /// <param name="expression">expression to resolve to an enum.</param>
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public EnumExpression(Expression expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpression{T}"/> class.
        /// </summary>
        /// <param name="lambda">function (data) which evaluates to enum.</param>
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public EnumExpression(Func<object, object> lambda)
            : this(Expression.Lambda(lambda))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpression{T}"/> class.
        /// </summary>
        /// <param name="expressionOrValue">JsonNode value to resolve to an enum.</param>
        [RequiresDynamicCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        [RequiresUnreferencedCode("For AOT compatibility, use overloads that take a JsonTypeInfo")]
        public EnumExpression(JsonNode expressionOrValue)
            : base(expressionOrValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpression{T}"/> class.
        /// </summary>
        /// <param name="typeInfo">typeInfo.</param>
        public EnumExpression(JsonTypeInfo typeInfo)
            : base(typeInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpression{T}"/> class.
        /// </summary>
        /// <param name="value">value of T.</param>
        /// <param name="typeInfo">typeInfo.</param>
        public EnumExpression(T value, JsonTypeInfo typeInfo)
            : base(value, typeInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpression{T}"/> class.
        /// </summary>
        /// <param name="value">value of T.</param>
        /// <param name="typeInfo">typeInfo.</param>
        public EnumExpression(JsonNode value, JsonTypeInfo typeInfo)
            : base(value, typeInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumExpression{T}"/> class.
        /// </summary>
        /// <param name="expression">expression to resolve to an enum.</param>
        /// <param name="typeInfo">typeInfo.</param>
        public EnumExpression(string expression, JsonTypeInfo typeInfo)
            : base(expression, typeInfo)
        {
        }

        /// <summary>
        /// Converts a value to an EnumExpression instance.
        /// </summary>
        /// <param name="value">The value to convert.</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
        [RequiresUnreferencedCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        [RequiresDynamicCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        public static implicit operator EnumExpression<T>(T value) => new EnumExpression<T>(value);

        /// <summary>
        /// Converts a string value to an EnumExpression instance.
        /// </summary>
        /// <param name="expressionOrValue">The string value.</param>
        [RequiresUnreferencedCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        [RequiresDynamicCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        public static implicit operator EnumExpression<T>(string expressionOrValue) => new EnumExpression<T>(expressionOrValue);

        /// <summary>
        /// Converts an Expression instance to an EnumExpression instance.
        /// </summary>
        /// <param name="expression">The Expression instance.</param>
        [RequiresUnreferencedCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        [RequiresDynamicCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        public static implicit operator EnumExpression<T>(Expression expression) => new EnumExpression<T>(expression);

        /// <summary>
        /// Converts a JSON Token to an EnumExpression instance.
        /// </summary>
        /// <param name="value">The JSON Token to convert.</param>
        [RequiresUnreferencedCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        [RequiresDynamicCode("Implicit operator can't infer JsonTypeInfo for T, use explicit constructor")]
        public static implicit operator EnumExpression<T>(JsonNode value) => new EnumExpression<T>(value);
#pragma warning restore CA2225 // Operator overloads have named alternates

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value">Value to set.</param>
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
