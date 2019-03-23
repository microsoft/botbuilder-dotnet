using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    public class Constant: Expression
    {
        public Constant(object value)
            : base(ExpressionType.Constant)
        {
            Value = value;
            Children = new List<Expression>();
        }

        public object Value { get; }

        public override (object value, string error) TryEvaluate(IReadOnlyDictionary<string, object> state)
        {
            return (Value, null);
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
