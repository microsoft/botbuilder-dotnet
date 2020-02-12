// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Expressions.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Expressions.Properties
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
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolExpression"/> class.
        /// </summary>
        /// <param name="value">bool value.</param>
        public BoolExpression(bool value) 
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolExpression"/> class.
        /// </summary>
        /// <param name="expression">expression to resolve to bool.</param>
        public BoolExpression(string expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolExpression"/> class.
        /// </summary>
        /// <param name="value">expression or value to resolve to bool.</param>
        public BoolExpression(JToken value)
            : base(value)
        {
        }

        public static implicit operator BoolExpression(bool value) => new BoolExpression(value);

        public static implicit operator BoolExpression(string value) => new BoolExpression(value);

        public static implicit operator BoolExpression(JToken value) => new BoolExpression(value);
    }
}
