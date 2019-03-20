using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Expressions
{
    public class Constant: Expression
    {
        public Constant(object value)
            : base(ExpressionType.Constant)
        {
            Value = value;
        }

        public object Value { get; }
    }
}
