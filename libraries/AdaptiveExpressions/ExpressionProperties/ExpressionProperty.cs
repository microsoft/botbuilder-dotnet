// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.Properties
{
    /// <summary>
    /// Base class which defines a Expression or value for a property.
    /// </summary>
    /// <typeparam name="T">type of object the expression should evaluate to.</typeparam>
    public class ExpressionProperty<T> : IExpressionProperty
    {
        private Expression expression;

        public ExpressionProperty()
        {
        }

        public ExpressionProperty(object value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Gets or sets the raw value of the expression property.
        /// </summary>
        /// <value>
        /// the value to return when someone calls GetValue().
        /// </value>
        public T Value { get; protected set; } = default(T);

        /// <summary>
        /// Gets or sets the expression text to evaluate to get the value.
        /// </summary>
        /// <value>
        /// The expression text.
        /// </value>
        public string ExpressionText { get; set; }

        public static implicit operator ExpressionProperty<T>(T value) => new ExpressionProperty<T>(value);

        public static implicit operator ExpressionProperty<T>(string expression) => new ExpressionProperty<T>(expression);

        public static implicit operator ExpressionProperty<T>(Expression expression) => new ExpressionProperty<T>(expression);

        public override string ToString()
        {
            if (this.ExpressionText != null)
            {
                return $"={this.ExpressionText.TrimStart('=')}";
            }

            return this.Value?.ToString();
        }

        /// <summary>
        /// This will return the existing expression or ConstantExpression(Value) if the value is non-complex type.
        /// </summary>
        /// <returns>expression.</returns>
        public virtual Expression ToExpression()
        {
            if (this.expression != null)
            {
                return expression;
            }

            if (this.ExpressionText != null)
            {
                this.expression = Expression.Parse(this.ExpressionText.TrimStart('='));
                return expression;
            }

            if (this.Value == null || this.Value is string || this.Value.IsNumber() || this.Value.IsInteger() || this.Value is bool || this.Value.GetType().IsEnum)
            {
                // return expression as constant
                this.expression = Expression.Parse(this.Value.ToString());
                return expression;
            }

            // return expression for json object
            this.expression = Expression.Parse($"json({JsonConvert.SerializeObject(this.Value)})");
            return expression;
        }

        /// <summary>
        /// Get the value.
        /// </summary>
        /// <param name="data">data to use for expression binding.</param>
        /// <returns>value or default(T) if not found.</returns>
        public virtual T GetValue(object data)
        {
            return this.TryGetValue(data).Value;
        }

        /// <summary>
        /// try to Get the value.
        /// </summary>
        /// <param name="data">data to use for expression binding.</param>
        /// <returns>value.</returns>
        public virtual (T Value, string Error) TryGetValue(object data)
        {
            if (expression == null && ExpressionText != null)
            {
                this.expression = Expression.Parse(this.ExpressionText.TrimStart('='));
            }

            if (expression != null)
            {
                return expression.TryEvaluate<T>(data);
            }

            return (Value, null);
        }

        /// <summary>
        /// Set the value.
        /// </summary>
        /// <param name="value">value to set.</param>
        public virtual void SetValue(object value)
        {
            this.expression = null;
            this.Value = default(T);
            this.ExpressionText = null;

            if (value == null)
            {
                return;
            }

            if (value is Expression exp)
            {
                this.expression = exp;
                this.ExpressionText = exp.ToString();
                return;
            }

            if (value is string stringOrExpression)
            {
                this.ExpressionText = stringOrExpression.TrimStart('=');
                return;
            }

            this.Value = ConvertObject(value);
        }

        /// <summary>
        /// Get value as object.
        /// </summary>
        /// <remarks>Helper methods which allows you to work with the expression property values as purely objects.</remarks>
        /// <param name="data">data to bind to.</param>
        /// <returns>value as object.</returns>
        public virtual object GetObject(object data)
        {
            return GetValue(data);
        }

        /// <summary>
        /// Try Get value as object.
        /// </summary>
        /// <remarks>Helper methods which allows you to work with the expression property values as purely objects.</remarks>
        /// <param name="data">data.</param>
        /// <returns>Value and error.</returns>
        public virtual (object Value, string Error) TryGetObject(object data)
        {
            return TryGetValue(data);
        }

        /// <summary>
        /// Set value as object.
        /// </summary>
        /// <param name="value">object.</param>
        public virtual void SetObject(object value)
        {
            SetValue(value);
        }

        /// <summary>
        /// Convert raw object to desired value type.
        /// </summary>
        /// <remarks>
        /// This method is called whenever an object is fected via expression or is deserialized from raw text.
        /// </remarks>
        /// <param name="result">result to convert to object of type T.</param>
        /// <returns>object of type T.</returns>
        protected virtual T ConvertObject(object result)
        {
            if (result is T)
            {
                return (T)result;
            }

            if (result == null)
            {
                return default(T);
            }

            return JToken.FromObject(result).ToObject<T>();
        }
    }
}
