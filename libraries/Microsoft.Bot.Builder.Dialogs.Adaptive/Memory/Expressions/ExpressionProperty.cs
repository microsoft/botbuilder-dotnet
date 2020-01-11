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
    /// Defines a Expression or value for a property.
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

        [JsonIgnore]
        public T Value { get; set; } = default(T);

        [JsonIgnore]
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
        /// Get the value.
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
