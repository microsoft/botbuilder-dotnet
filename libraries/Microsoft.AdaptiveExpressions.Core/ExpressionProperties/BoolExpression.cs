﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AdaptiveExpressions.Core.Converters;

namespace Microsoft.AdaptiveExpressions.Core.Properties
{
    /// <summary>
    /// BoolExpression - represents a property which is either a boolean or a string expression which resolves to a boolean.
    /// </summary>
    /// <remarks>String values are always interpreted as an expression, whether it has '=' prefix or not.</remarks>
    [JsonConverter(typeof(BoolExpressionConverter))]
    public class BoolExpression : ExpressionProperty<bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoolExpression"/> class.
        /// </summary>
        public BoolExpression()
            : base(AdaptiveExpressionsSerializerContext.Default.Boolean)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolExpression"/> class.
        /// </summary>
        /// <param name="value">bool value.</param>
        public BoolExpression(bool value) 
            : base(value, AdaptiveExpressionsSerializerContext.Default.Boolean)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolExpression"/> class.
        /// </summary>
        /// <param name="expression">expression to resolve to bool.</param>
        public BoolExpression(string expression)
            : base(expression, AdaptiveExpressionsSerializerContext.Default.Boolean)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolExpression"/> class.
        /// </summary>
        /// <param name="expression">expression to resolve to bool.</param>
        public BoolExpression(Expression expression)
            : base(expression, AdaptiveExpressionsSerializerContext.Default.Boolean)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolExpression"/> class.
        /// </summary>
        /// <param name="lambda">function (data) which evaluates to bool.</param>
        public BoolExpression(Func<object, object> lambda)
            : this(Expression.Lambda(lambda))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolExpression"/> class.
        /// </summary>
        /// <param name="expressionOrValue">expression or value to resolve to bool.</param>
        public BoolExpression(JsonNode expressionOrValue)
            : base(expressionOrValue, AdaptiveExpressionsSerializerContext.Default.Boolean)
        {
        }

        /// <summary>
        /// Converts a boolean value to a BoolExpression.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static implicit operator BoolExpression(bool value) => new BoolExpression(value);

        /// <summary>
        /// Converts a string value to a BoolExpression.
        /// </summary>
        /// <param name="expression">The string value to convert.</param>
        public static implicit operator BoolExpression(string expression) => new BoolExpression(expression);

        /// <summary>
        /// Converts an Expression instance to BoolExpression.
        /// </summary>
        /// <param name="expression">The Expression instance to convert.</param>
        public static implicit operator BoolExpression(Expression expression) => new BoolExpression(expression);

        /// <summary>
        /// Converts a JSON Token to BoolExpression.
        /// </summary>
        /// <param name="expressionOrValue">The JSON Token to Convert.</param>
        public static implicit operator BoolExpression(JsonNode expressionOrValue) => new BoolExpression(expressionOrValue);
#pragma warning restore CA2225 // Operator overloads have named alternates
    }
}
