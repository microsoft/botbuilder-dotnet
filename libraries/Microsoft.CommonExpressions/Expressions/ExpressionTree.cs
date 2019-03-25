using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    public class ExpressionTree : Expression
    {
        protected ExpressionTree(string type, IEnumerable<Expression> children, IExpressionEvaluator evaluator = null)
            : base(type, evaluator)
        {
            Children = children.ToList();
        }

        public IReadOnlyList<Expression> Children { get; }

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

        public static ExpressionTree MakeExpressionTree(string type, IEnumerable<Expression> children, IExpressionEvaluator evaluator = null)
        {
            var expr = new ExpressionTree(type, children, evaluator);
            expr.Validate();
            return expr;
        }
    }
}
