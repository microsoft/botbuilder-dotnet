using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Expressions
{
    public class Unary : Expression
    {
        public Unary(string type, Expression child)
            : base(type)
        {
            Child = child;
        }

        public Expression Child { get; }

        public override string ToString()
        {
            return $"{Type}({Child})";
        }
    }
}
