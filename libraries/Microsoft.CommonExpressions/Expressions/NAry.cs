using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    public class NAry : Expression
    {
        public NAry(string type, IEnumerable<Expression> children)
            : base(type)
        {
            Children = children.ToArray();
        }

        public Expression[] Children { get; }

        protected virtual ExpressionEvaluator GetNAryEvaluator()
        {
            return BuiltInFunctions.GetNAryEvaluator(Type);
        }

        public override async Task<object> Evaluate(IDictionary<string, object> state)
        {
            var args = new List<object>();
            foreach(var child in Children)
            {
                args.Add(child.Evaluate(state));
            }
            return await GetNAryEvaluator()(args);
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

            return builder.ToString();
        }
    }
}
