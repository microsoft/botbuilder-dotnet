using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    public class Binary : Expression
    {
        public Binary(string type, Expression left, Expression right)
            : base(type)
        {
            Left = left;
            Right = right;
            GetBinaryEvaluator();
        }

        public Expression Left { get; }

        public Expression Right { get; }

        protected virtual ExpressionEvaluator GetBinaryEvaluator()
        {
            return BuiltInFunctions.GetBinaryEvaluator(Type);
        }

        public override async Task<object> Evaluate(IDictionary<string, object> state)
        {
            return await GetBinaryEvaluator()(new List<object> { await Left.Evaluate(state), await Right.Evaluate(state) });
        }

        public override string ToString()
        {
            return $"{Type}({Left}, {Right})";
        }
    }
}
