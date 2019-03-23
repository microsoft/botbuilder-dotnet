using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    /// <summary>
    /// Delegate which evaluates operators operands (aka the paramters) to the result
    /// </summary>
    public delegate (object value, string error) ExpressionEvaluator(
        IReadOnlyList<Expression> parameters, 
        IReadOnlyDictionary<string, object> state);

    public abstract class Expression : IExpression
    {
        public Expression(string type)
        {
            Type = type;
        }

        public string Type { get; }

        public IReadOnlyList<Expression> Children { get; protected set; }

        public abstract (object value, string error) TryEvaluate(IReadOnlyDictionary<string, object> vars);

        public virtual void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

    }
}
