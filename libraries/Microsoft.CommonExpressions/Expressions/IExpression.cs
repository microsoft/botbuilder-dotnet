using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    public interface IExpression
    {
        /// <summary>
        /// Evaluate and return the value or an error message.
        /// </summary>
        /// <param name="state">State for evaluation can be object or IDictionary.</param>
        /// <returns>Computed value and error message or null if none.</returns>
        (object value, string error) TryEvaluate(object state);
    }
}
