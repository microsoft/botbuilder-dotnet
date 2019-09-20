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
    public class ExpressionProperty<T>
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
            this.Value = ConvertObject(value);
        }

        public ExpressionProperty(T value)
        {
            this.Value = value;
        }

        public string Expression
        {
            get { return expression.ToString(); }
            set { expression = new ExpressionEngine().Parse(value); }
        }

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

        protected virtual T ConvertObject(object result)
        {
            return JObject.FromObject(result).ToObject<T>();
        }
    }
}
