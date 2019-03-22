using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    /// Delegate which evaluates operators operands (aka the paramters) to the result
    /// </summary>
    public delegate Task<object> ExpressionEvaluator(IReadOnlyList<object> parameters);

    public abstract class Expression
    {
        public Expression(string type)
        {
            Type = type;
        }

        public string Type { get; }

        public abstract Task<object> Evaluate(IDictionary<string, object> state);
    }
}
