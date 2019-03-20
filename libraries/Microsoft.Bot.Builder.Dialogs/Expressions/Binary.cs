using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Expressions
{
    public class Binary : Expression
    {
        public Binary(string type, Expression left, Expression right)
            : base(type)
        {
            Left = left;
            Right = right;
        }

        public Expression Left { get; }

        public Expression Right { get; }

        public override string ToString()
        {
            return $"{Type}({Left}, {Right})";
        }
    }
}
