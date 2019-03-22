using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Expressions
{
    public class Call : NAry
    {

        public Call(ExpressionEvaluator function, IEnumerable<Expression> args, string name)
            : base(ExpressionType.Call, args)
        {
            Function = function;
            Name = name;
        }

        public ExpressionEvaluator Function { get; }

        public string Name { get; }

        protected override ExpressionEvaluator GetNAryEvaluator()
        {
            return Function;
        }

        public override string ToString()
        {
            return ToString(Name);
        }
    }
}
