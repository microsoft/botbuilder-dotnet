using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Trim : StringTransformEvaluator
    {
        public Trim()
            : base(ExpressionType.Trim, Function)
        {
        }

        private static object Function(IReadOnlyList<object> args)
        {
            if (args[0] == null)
            {
                return string.Empty;
            }
            else
            {
                return args[0].ToString().Trim();
            }
        }
    }
}
