// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Expressions.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Expressions.Properties
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
        /// <param name="value">JToken to resolve to an int.</param>
        public IntExpression(JToken value)
            : base(value)
        {
        }

        public static implicit operator IntExpression(int value) => new IntExpression(value);

        public static implicit operator IntExpression(string value) => new IntExpression(value);

        public static implicit operator IntExpression(JToken value) => new IntExpression(value);
    }
}
