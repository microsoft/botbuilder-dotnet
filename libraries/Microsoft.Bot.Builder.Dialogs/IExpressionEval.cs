using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Expressions
{
    public interface IExpressionEval
    {
        Task<object> Evaluate(IDictionary<String, object> vars);

        Task<object> Evaluate(string expression, IDictionary<String, object> vars);
    }

}
