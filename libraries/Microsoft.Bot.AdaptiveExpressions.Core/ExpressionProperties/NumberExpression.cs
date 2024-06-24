// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Bot.AdaptiveExpressions.Core.Converters;

namespace Microsoft.Bot.AdaptiveExpressions.Core.Properties
{
    /// <summary>
    /// NumberExpression - represents a property which is either a float or a string expression which resolves to a float.
    /// </summary>
    /// <remarks>String values are always interpreted as an expression, whether it has '=' prefix or not.</remarks>
    [JsonConverter(typeof(NumberExpressionConverter))]
    public class NumberExpression : ExpressionProperty<double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        public NumberExpression()
            : base(AdaptiveExpressionsSerializerContext.Default.Double)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        /// <param name="value">value to use.</param>
        public NumberExpression(double value) 
            : base(value, AdaptiveExpressionsSerializerContext.Default.Double)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        /// <param name="expression">string to interpret as expression or number.</param>
        public NumberExpression(string expression)
            : base(expression, AdaptiveExpressionsSerializerContext.Default.Double)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberExpression"/> class.
        /// </summary>
        /// <param name="expression">expression.</param>
        public NumberExpression(Expression expression)
            : base(expression, AdaptiveExpressionsSerializerContext.Default.Double)
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
        /// <param name="expressionOrValue">JsonNode to interpret as expression or number.</param>
        public NumberExpression(JsonNode expressionOrValue)
            : base(expressionOrValue, AdaptiveExpressionsSerializerContext.Default.Double)
        {
        }

        /// <summary>
        /// Converts a floating point number value to a NumberExpression instance.
        /// </summary>
        /// <param name="value">The floating point number number to convert.</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static implicit operator NumberExpression(double value) => new NumberExpression(value);

        /// <summary>
        /// Converts a string value to a NumberExpression instance.
        /// </summary>
        /// <param name="expression">The string value to convert.</param>
        public static implicit operator NumberExpression(string expression) => new NumberExpression(expression);

        /// <summary>
        /// Converts an Expression instance to a NumberExpression instance.
        /// </summary>
        /// <param name="expression">The Expression instance to convert.</param>
        public static implicit operator NumberExpression(Expression expression) => new NumberExpression(expression);

        /// <summary>
        /// Converts a JSON Token to an NumberExpression instance.
        /// </summary>
        /// <param name="expressionOrValue">The JSON Token to convert.</param>
        public static implicit operator NumberExpression(JsonNode expressionOrValue) => new NumberExpression(expressionOrValue);
#pragma warning restore CA2225 // Operator overloads have named alternates
    }
}
