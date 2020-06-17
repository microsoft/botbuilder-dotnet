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

        public static implicit operator ValueExpression(string valueOrExpression) => new ValueExpression(valueOrExpression);

        public static implicit operator ValueExpression(int value) => new ValueExpression(value);

        public static implicit operator ValueExpression(long value) => new ValueExpression(value);

        public static implicit operator ValueExpression(float value) => new ValueExpression(value);

        public static implicit operator ValueExpression(double value) => new ValueExpression(value);

        public static implicit operator ValueExpression(DateTime value) => new ValueExpression(value);

        public static implicit operator ValueExpression(bool value) => new ValueExpression(value);

        public static implicit operator ValueExpression(JToken valueOrExpression) => new ValueExpression(valueOrExpression);
        
        public static implicit operator ValueExpression(Expression expression) => new ValueExpression(expression);

        public override void SetValue(object value)
        {
            var stringOrExpression = (value as string) ?? (value as JValue)?.Value as string;
            this.ExpressionText = null;
            this.Value = null;

            if (stringOrExpression != null)
            {
                // if it starts with = it always is an expression
                if (stringOrExpression.StartsWith("="))
                {
                    ExpressionText = stringOrExpression;
                    return;
                }
                else if (stringOrExpression.StartsWith("\\="))
                {
                    // then trim off the escape char for equals (\=foo) should simply be the string (=foo), and not an expression (but it could still be stringTemplate)
                    stringOrExpression = stringOrExpression.TrimStart('\\');
                }

                // keep the string as quoted expression, which will be literal unless string interpolation is used.
                this.ExpressionText = $"=`{stringOrExpression}`";
                return;
            }

            base.SetValue(value);
        }
    }
}
