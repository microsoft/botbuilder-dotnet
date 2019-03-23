using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Expressions
{
    public interface IExpression
    {
        Task<object> Evaluate(IDictionary<string, object> vars);
    }
}
