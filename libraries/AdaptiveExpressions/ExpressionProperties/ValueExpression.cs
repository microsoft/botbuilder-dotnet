// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.Properties
{
    /// <summary>
    /// ValueExpression - represents a property which is an object of any kind or a string expression.
    /// </summary>
    /// <remarks>
    /// If the value is 
    /// * a string with '=' prefix then the string is treated as an expression to resolve to a string. 
    /// * a string without '=' then value is treated as string with string interpolation.
    /// * any other type, then it is of that type (int, bool, object, etc.)
    /// You can escape the '=' prefix by putting a backslash.  
    /// Examples: 
    ///     prop = true ==> true
    ///     prop = "Hello ${user.name}" => "Hello Joe"
    ///     prop = "=length(user.name)" => 3
    ///     prop = "=user.age" => 45.
    ///     prop = "\=user.age" => "=user.age".
    /// </remarks>
    [JsonConverter(typeof(ValueExpressionConverter))]
    public class ValueExpression : ExpressionProperty<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueExpression"/> class.
        /// </summary>
        public ValueExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueExpression"/> class.
        /// </summary>
        /// <param name="value">value to interpret as object or string expression.</param>
        public ValueExpression(object value)
            : base(value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueExpression"/> class.
        /// </summary>
        /// <param name="lambda">function (data) which evaluates to value.</param>
        public ValueExpression(Func<object, object> lambda)
            : this(Expression.Lambda(lambda))
        {
        }

        /// <summary>
        /// Converts a string value to a ValueExpression instance.
        /// </summary>
        /// <param name="valueOrExpression">A string value to convert.</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static implicit operator ValueExpression(string valueOrExpression) => new ValueExpression(valueOrExpression);

        /// <summary>
        /// Converts an integer value to a ValueExpression instance.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        public static implicit operator ValueExpression(int value) => new ValueExpression(value);

        /// <summary>
        /// Converts a long integer value to a ValueExpression instance.
        /// </summary>
        /// <param name="value">The long integer value to convert.</param>
        public static implicit operator ValueExpression(long value) => new ValueExpression(value);

        /// <summary>
        /// Converts a floating point number value to a ValueExpression instance.
        /// </summary>
        /// <param name="value">The floating ponit number value to convert.</param>
        public static implicit operator ValueExpression(float value) => new ValueExpression(value);

        /// <summary>
        /// Converts a double precision floating number value to a ValueExpression instance.
        /// </summary>
        /// <param name="value">The double precision floating number value to convert.</param>
        public static implicit operator ValueExpression(double value) => new ValueExpression(value);

        /// <summary>
        /// Converts a DateTime value to a ValueExpression instance.
        /// </summary>
        /// <param name="value">The DateTime value to convert.</param>
        public static implicit operator ValueExpression(DateTime value) => new ValueExpression(value);

        /// <summary>
        /// Converts a boolean value to a ValueExpression instance.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        public static implicit operator ValueExpression(bool value) => new ValueExpression(value);

        /// <summary>
        /// Converts a JSON Token to a ValueExpression instance.
        /// </summary>
        /// <param name="valueOrExpression">The JSON Token to convert.</param>
        public static implicit operator ValueExpression(JToken valueOrExpression) => new ValueExpression(valueOrExpression);

        /// <summary>
        /// Converts an Expression instance to a ValueExpression instance.
        /// </summary>
        /// <param name="expression">The Expression instance to convert.</param>
        public static implicit operator ValueExpression(Expression expression) => new ValueExpression(expression);
#pragma warning restore CA2225 // Operator overloads have named alternates

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public override void SetValue(object value)
        {
            var stringOrExpression = (value as string) ?? (value as JValue)?.Value as string;
            this.ExpressionText = null;
            this.Value = null;

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
                this.ExpressionText = $"=`{stringOrExpression.Replace("\\", "\\\\")}`";
                return;
            }

            base.SetValue(value);
        }
    }
}
