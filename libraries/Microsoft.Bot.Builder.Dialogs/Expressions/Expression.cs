using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Expressions
{
    public abstract class Expression
    {
        public Expression(string type)
        {
            Type = type;
        }

        public string Type { get; }

        // TODO: Do I need these?
        public static Binary Add(Expression left, Expression right) => new Binary(ExpressionType.Add, left, right);
    }
}
