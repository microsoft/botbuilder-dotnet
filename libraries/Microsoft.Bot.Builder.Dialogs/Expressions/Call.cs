using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Expressions
{
    public class Call : NAry
    {
        public Call(string function, IEnumerable<Expression> args)
            : base(ExpressionType.Call, args)
        {
            Function = function;
        }

        public string Function { get; }

        public override string ToString()
        {
            return base.ToString(Function);
        }
    }
}
