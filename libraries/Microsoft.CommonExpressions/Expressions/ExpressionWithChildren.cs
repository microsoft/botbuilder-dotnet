using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Expressions
{
    public class ExpressionWithChildren : Expression
    {
        protected ExpressionWithChildren(string type, IEnumerable<Expression> children = null, IExpressionEvaluator evaluator = null)
            : base(type, evaluator)
        {
            if (children == null)
            {
                _children = new List<Expression>();
            }
            else
            {
                _children = children.ToList();
            }
        }

        public IReadOnlyList<Expression> Children { get => _children; }

        protected List<Expression> _children;

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return ToString(Type);
        }

        protected string ToString(string name)
        {
            var builder = new StringBuilder();
            builder.Append(Type);
            builder.Append('(');
            var first = true;
            foreach (var child in Children)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(", ");
                }

                builder.Append(child.ToString());
            }
            builder.Append(')');
            return builder.ToString();
        }

        public static ExpressionWithChildren MakeExpression(string type, IEnumerable<Expression> children, IExpressionEvaluator evaluator = null)
        {
            var expr = new ExpressionWithChildren(type, children, evaluator);
            expr.Validate();
            return expr;
        }
    }
}
