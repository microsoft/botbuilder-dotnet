// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.Properties
{
    /// <summary>
    /// StringExpression - represents a property which is either a string value or a string expression.
    /// </summary>
    /// <remarks>
    /// If the value is 
    /// * a string with '=' prefix then the string is treated as an expression to resolve to a string. 
    /// * a string without '=' then value is treated as string with string interpolation.
    /// * You can escape the '=' prefix by putting a backslash.  
    /// Examples: 
    ///     prop = "Hello ${user.name}" => "Hello Joe"
    ///     prop = "=length(user.name)" => "3"
    ///     prop = "=user.name" => "Joe"
    ///     prop = "\=user" => "=user".
    /// </remarks>
    [JsonConverter(typeof(StringExpressionConverter))]
    public class StringExpression : ExpressionProperty<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringExpression"/> class.
        /// </summary>
        public StringExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringExpression"/> class.
        /// </summary>
        /// <param name="expressionOrValue">string to interpret as string or expression to a string.</param>
        public StringExpression(string expressionOrValue)
            : base(expressionOrValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringExpression"/> class.
        /// </summary>
        /// <param name="expressionOrValue">value to interpret as a string or expression to a string.</param>
        public StringExpression(JToken expressionOrValue)
            : base(expressionOrValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringExpression"/> class.
        /// </summary>
        /// <param name="expression">expression to a string.</param>
        public StringExpression(Expression expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringExpression"/> class.
        /// </summary>
        /// <param name="lambda">function (data) which evaluates to string.</param>
        public StringExpression(Func<object, object> lambda)
            : this(Expression.Lambda(lambda))
        {
        }

        /// <summary>
        /// Converts a string value to a StringExpression instance.
        /// </summary>
        /// <param name="valueOrExpression">The string value to convert.</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static implicit operator StringExpression(string valueOrExpression) => new StringExpression(valueOrExpression);

        /// <summary>
        /// Converts a JSON Token to a StringExpression instance.
        /// </summary>
        /// <param name="valueOrExpression">The JSON Token to convert.</param>
        public static implicit operator StringExpression(JToken valueOrExpression) => new StringExpression(valueOrExpression);

        /// <summary>
        /// Converts an Expression instance to a StringExpression instance.
        /// </summary>
        /// <param name="expression">The Expression instance to convert.</param>
        public static implicit operator StringExpression(Expression expression) => new StringExpression(expression);
#pragma warning restore CA2225 // Operator overloads have named alternates

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public override void SetValue(object value)
        {
            // reset state to no value or expression.
            base.SetValue(null);

            if (value is Expression exp)
            {
                base.SetValue(value);
                return;
            }

            var stringOrExpression = (value as string) ?? (value as JValue)?.Value as string;
            if (stringOrExpression != null)
            {
                // if it starts with = it always is an expression
                if (stringOrExpression.StartsWith("=", StringComparison.Ordinal))
                {
                    ExpressionText = stringOrExpression;
                    return;
                }
                else if (stringOrExpression.StartsWith("\\=", StringComparison.Ordinal))
                {
                    // then trim off the escape char for equals (\=foo) should simply be the string (=foo), and not an expression (but it could still be stringTemplate)
                    stringOrExpression = stringOrExpression.TrimStart('\\');
                }

                // keep the string as quoted expression, which will be literal unless string interpolation is used.
                this.ExpressionText = $"=`{stringOrExpression.Replace("\\", "\\\\").Replace("`", "\\`")}`";
                return;
            }
        }
    }
}
