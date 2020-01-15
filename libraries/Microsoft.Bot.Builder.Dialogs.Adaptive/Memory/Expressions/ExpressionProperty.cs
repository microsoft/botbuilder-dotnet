// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Text;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Base class which defines a Expression or value for a property.
    /// </summary>
    /// <typeparam name="T">type of object the expression should evaluate to.</typeparam>
    public class ExpressionProperty<T>
    {
        public ExpressionProperty()
        {
        }

        public ExpressionProperty(object value)
        {
            SetValue(value);
        }

        public T Value { get; set; } = default(T);

        public Expression Expression { get; set; }

        public new string ToString()
        {
            if (this.Expression != null)
            {
                return $"={this.Expression}";
            }

            return this.Value?.ToString();
        }

        /// <summary>
        /// This will return the existing expression or ConstantExpression(Value) if the value is non-complex type.
        /// </summary>
        /// <returns>expression.</returns>
        public Expression ToExpression()
        {
            if (this.Expression != null)
            {
                return this.Expression;
            }

            if (this.Value is string || this.Value.IsNumber() || this.Value.IsInteger() || this.Value is bool || this.Value.GetType().IsEnum)
            {
                return new ExpressionEngine().Parse(this.Value.ToString());
            }

            // return expression for json object
            return new ExpressionEngine().Parse($"json({JsonConvert.SerializeObject(this.Value)})");
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
            if (Expression != null)
            {
                return Expression.TryEvaluate<T>(data);
            }

            return (Value, null);
        }

        /// <summary>
        /// Set the value.
        /// </summary>
        /// <param name="value">value to set.</param>
        public virtual void SetValue(object value)
        {
            this.Value = default(T);
            this.Expression = null;

            if (value == null)
            {
                this.Value = default(T);
                this.Expression = null;
                return;
            }

            if (value is string stringOrExpression)
            {
                Expression = new ExpressionEngine().Parse(stringOrExpression.TrimStart('='));
                return;
            }

            this.Value = ConvertObject(value);
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
