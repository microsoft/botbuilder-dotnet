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
            Children = new List<Expression>() { left, right };
            GetBinaryEvaluator();
        }

        public Expression Left { get; }

        public Expression Right { get; }

        protected virtual ExpressionEvaluator GetBinaryEvaluator()
        {
            return BuiltInFunctions.GetBinaryEvaluator(Type);
        }

        public override (object value, string error) TryEvaluate(IReadOnlyDictionary<string, object> state)
        {
            return GetBinaryEvaluator()(Children, state);
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"{Type}({Left}, {Right})";
        }
    }
}
