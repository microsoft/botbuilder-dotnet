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
            Children = new List<Expression>() { child };
        }

        public Expression Child { get; }

        protected virtual ExpressionEvaluator GetUnaryEvaluator()
        {
            return BuiltInFunctions.GetUnaryEvaluator(Type);
        }

        public override (object value, string error) TryEvaluate(IReadOnlyDictionary<string, object> state)
        {
            return GetUnaryEvaluator()(Children, state);
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"{Type}({Child})";
        }
    }
}
