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
        }

        public object Value { get; }

        public override Task<object> Evaluate(IDictionary<string, object> state)
        {
            return Task.FromResult(Value);
        }
    }
}
