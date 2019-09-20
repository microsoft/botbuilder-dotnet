using System.Threading.Tasks;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    /// <summary>
    /// Defines a Expression or value for a property
    /// </summary>
    /// <typeparam name="T">type of object the expression should evaluate to</typeparam>
    public class ExpressionProperty<T> : IExpressionProperty
    {
        protected Expression expression;

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

        public void SetValue(object value)
        {
            if (value is string expression)
            {
                this.Expression = expression;
            }
            else
            {
                this.Value = ConvertObject(value);
            }
        }

        /// <summary>
        /// Gets or sets expression to use to get the value from data
        /// </summary>
        public string Expression
        {
            get { return expression.ToString(); }
            set { expression = new ExpressionEngine().Parse(value); }
        }

        /// <summary>
        /// Gets or sets static value to use for the result (instead of data binding)
        /// </summary>
        public T Value { get; set; }

        public virtual T GetValue(object data)
        {
            if (Value != null)
            {
                return Value;
            }

            var (result, error) = expression.TryEvaluate(data);
            if (error != null)
            {
                return default(T);
            }

            return ConvertObject(result);
        }

        /// <summary>
        /// Convert raw object to desired value type
        /// </summary>
        /// <remarks>
        /// This method is called whenever an object is fected via expression or is deserialized from raw text.
        /// </remarks>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual T ConvertObject(object result)
        {
            if (result is T)
            {
                return (T)result;
            }

            return JObject.FromObject(result).ToObject<T>();
        }
    }
}
