using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;

namespace Microsoft.Bot.Builder.Dialogs.Expressions
{
    public interface IExpressionEval
    {
        Task<object> Evaluate(DialogContextState state);

        Task<object> Evaluate(IDictionary<string, object> vars);

        Task<object> Evaluate(string expression, IDictionary<string, object> vars);

        Expression Parse();
    }

}
