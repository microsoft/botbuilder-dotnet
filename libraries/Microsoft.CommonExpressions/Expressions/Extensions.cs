using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    public static partial class Extensions
    {
        public static (object value, string error) TryEvaluate(this Expression expression, object state)
        {
            return expression.TryEvaluate(new ReflectionDictionary(state));
        }
     }
}
