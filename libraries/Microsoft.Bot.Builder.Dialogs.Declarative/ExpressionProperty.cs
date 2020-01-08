// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    /// <summary>
    /// Defines a Expression or value for a property.
    /// </summary>
    /// <typeparam name="T">type of object the expression should evaluate to.</typeparam>
    public class ExpressionProperty<T> : IExpressionProperty
    {
        public ExpressionProperty()
        {
        }

        public ExpressionProperty(string expression)
        {
            this.Expression = expression;
        }

        public ExpressionProperty(object value)
        {
            SetValue(value);
        }

        public ExpressionProperty(T value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets expression to use to get the value from data.
        /// </summary>
        /// <value>
        /// Expression to use to get the value from data.
        /// </value>
        [JsonProperty("expression")]
        public string Expression
        {
            get { return InnerExpression?.ToString(); }
            set { InnerExpression = new ExpressionEngine().Parse(value); }
        }

        /// <summary>
        /// Gets or sets static value to use for the result (instead of data binding).
        /// </summary>
        /// <value>
        /// Static value to use for the result (instead of data binding).
        /// </value>
        [JsonProperty("value")]
        public T Value { get; set; }

        protected Expression InnerExpression { get; set; }

        /// <summary>
        /// Set the value.
        /// </summary>
        /// <param name="value">value to set.</param>
        public virtual void SetValue(object value)
        {
            if (value is string expression)
            {
                this.Expression = expression;
            }
            else if (value is JObject job)
            {
                dynamic jobj = job;
                if (jobj.value != null)
                {
                    this.Value = ConvertObject(jobj.value);
                }
                else if (jobj.expression != null)
                {
                    this.Expression = jobj.expression.ToString();
                }
                else
                {
                    this.Value = ConvertObject(job);
                }
            }
            else if (value is JArray jar)
            {
                this.Value = ConvertObject(jar);
            }
        }

        /// <summary>
        /// Get the value.
        /// </summary>
        /// <param name="data">data to use for expression binding.</param>
        /// <returns>value.</returns>
        public virtual T GetValue(object data)
        {
            if (Value != null)
            {
                return Value;
            }

            var (result, error) = InnerExpression.TryEvaluate(data);
            if (error != null)
            {
                return default(T);
            }

            return ConvertObject(result);
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

            return JToken.FromObject(result).ToObject<T>();
        }
    }
}
