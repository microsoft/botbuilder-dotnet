#pragma warning disable SA1401 // Fields should be private
using System.Collections.Generic;
using Microsoft.Bot.Builder.Expressions;

namespace Microsoft.Bot.Builder.AI.TriggerTrees.Tests
{
    public class ExpressionInfo
    {
        public Expression Expression;
        public Dictionary<string, Comparison> Bindings = new Dictionary<string, Comparison>();
        public List<Quantifier> Quantifiers = new List<Quantifier>();

        public ExpressionInfo(Expression expression)
        {
            Expression = expression;
        }

        public ExpressionInfo(Expression expression, string name, object value, string type)
        {
            Expression = expression;
            Bindings.Add(name, new Comparison(type, value));
        }

        public ExpressionInfo(Expression expression, Dictionary<string, Comparison> bindings, List<Quantifier> quantifiers = null)
        {
            Expression = expression;
            Bindings = bindings;
            if (quantifiers != null)
            {
                Quantifiers = quantifiers;
            }
        }

        public override string ToString() => Expression.ToString();
    }
}
