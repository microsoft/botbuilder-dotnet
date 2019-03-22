using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    public class Unary : Expression
    {
        public Unary(string type, Expression child)
            : base(type)
        {
            Child = child;
            GetUnaryEvaluator();
        }

        public Expression Child { get; }

        protected virtual ExpressionEvaluator GetUnaryEvaluator()
        {
            return BuiltInFunctions.GetUnaryEvaluator(Type);
        }

        public override async Task<object> Evaluate(IDictionary<string, object> state)
        {
            return await GetUnaryEvaluator()(new List<object> { await Child.Evaluate(state) });
        }

        public override string ToString()
        {
            return $"{Type}({Child})";
        }
    }
}
