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
        /// <param name="vars">State for evaluation.</param>
        /// <returns>Computed value and error message or null if none.</returns>
        (object value, string error) TryEvaluate(IReadOnlyDictionary<string, object> vars);
    }
}
