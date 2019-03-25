using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    public class Constant : Expression
    {
        protected Constant(object value)
            : base(ExpressionType.Constant,
                  new ExpressionEvaluator((expression, state) => ((expression as Constant).Value, null),
                      (value is string ? ExpressionReturnType.String
                      : value.IsNumber() ? ExpressionReturnType.Number
                      : value is Boolean ? ExpressionReturnType.Boolean
                      : ExpressionReturnType.Object),
                      (expression) => { }))
        {
            Value = value;
        }

        public object Value { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static Constant MakeConstant(object value)
            => new Constant(value);

    }
}
