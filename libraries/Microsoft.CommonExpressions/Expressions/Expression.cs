using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    public enum ExpressionReturnType
    {
        Boolean
        , Number
        , Object
        , String
    }

    public class Expression : IExpression
    {
        protected Expression(string type, IExpressionEvaluator evaluator = null)
        {
            Type = type;
            _evaluator = evaluator ?? BuiltInFunctions.Lookup(type);
        }

        public string Type { get; }

        public ExpressionReturnType ReturnType { get { return _evaluator.ReturnType; } }

        protected IExpressionEvaluator _evaluator { get; }

        public void Validate()
        {
            _evaluator.ValidateExpression(this);
        }

        public (object value, string error) TryEvaluate(object state)
            => _evaluator.TryEvaluate(this, state);

        public virtual void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public static Expression MakeExpression(string type, IExpressionEvaluator evaluator = null)
        {
            var expr = new Expression(type, evaluator);
            expr.Validate();
            return expr;
        }
    }
}
