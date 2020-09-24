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
        private Expression _expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionProperty{T}"/> class.
        /// </summary>
        public ExpressionProperty()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionProperty{T}"/> class.
        /// </summary>
        /// <param name="value">An object containing the value to be set.</param>
        public ExpressionProperty(object value)
        {
#pragma warning disable CA2214 // Do not call overridable methods in constructors (fixing this would require further redesign of this class and derived types, excluding it for now).
            SetValue(value);
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        /// <summary>
        /// Gets or sets the raw value of the expression property.
        /// </summary>
        /// <value>
        /// The value to return when someone calls GetValue().
        /// </value>
#pragma warning disable CA1721 // Property names should not match get methods (by design and we can't change it because of binary compat)
        public T Value { get; protected set; } = default(T);
#pragma warning restore CA1721 // Property names should not match get methods

        /// <summary>
        /// Gets or sets the expression text to evaluate to get the value.
        /// </summary>
        /// <value>
        /// The expression text.
        /// </value>
        public string ExpressionText { get; set; }

        /// <summary>
        /// Converts a value to an ExpressionProperty instance.
        /// </summary>
        /// <param name="value">A value to convert.</param>
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static implicit operator ExpressionProperty<T>(T value) => new ExpressionProperty<T>(value);

        /// <summary>
        /// Converts a string value to an ExpressionProperty instance.
        /// </summary>
        /// <param name="expression">The string value to convert.</param>
        public static implicit operator ExpressionProperty<T>(string expression) => new ExpressionProperty<T>(expression);

        /// <summary>
        /// Converts an Expression instance to an ExpressionProperty instance.
        /// </summary>
        /// <param name="expression">The Expression instance to convert.</param>
        public static implicit operator ExpressionProperty<T>(Expression expression) => new ExpressionProperty<T>(expression);
#pragma warning restore CA2225 // Operator overloads have named alternates

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string value.</returns>
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
            if (_expression != null)
            {
                return _expression;
            }

            if (this.ExpressionText != null)
            {
                _expression = Expression.Parse(this.ExpressionText.TrimStart('='));
                return _expression;
            }

            if (this.Value == null || this.Value is string || this.Value.IsNumber() || this.Value.IsInteger() || this.Value is bool || this.Value.GetType().IsEnum)
            {
                // return expression as constant
                _expression = Expression.Parse(this.Value.ToString());
                return _expression;
            }

            // return expression for json object
            _expression = Expression.Parse($"json({JsonConvert.SerializeObject(this.Value)})");
            return _expression;
        }

        /// <summary>
        /// Get the value.
        /// </summary>
        /// <param name="data">data to use for expression binding.</param>
        /// <returns>Value or default(T) if not found.</returns>
        public virtual T GetValue(object data)
        {
            return this.TryGetValue(data).Value;
        }

        /// <summary>
        /// Try to Get the value.
        /// </summary>
        /// <param name="data">data to use for expression binding.</param>
        /// <returns>value.</returns>
        public virtual (T Value, string Error) TryGetValue(object data)
        {
            if (_expression == null && ExpressionText != null)
            {
                _expression = Expression.Parse(this.ExpressionText.TrimStart('='));
            }

            if (_expression != null)
            {
                return _expression.TryEvaluate<T>(data);
            }

            return (Value, null);
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public virtual void SetValue(object value)
        {
            _expression = null;
            this.Value = default(T);
            this.ExpressionText = null;

            if (value == null)
            {
                return;
            }

            if (value is Expression exp)
            {
                _expression = exp;
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
