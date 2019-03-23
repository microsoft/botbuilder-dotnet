using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Expressions
{
    public class FunctionExpression  : IExpression
    {
        private Func<IDictionary<string, object>, Task<object>> eval;

        internal FunctionExpression(Func<IDictionary<string, object>, Task<object>> eval)
        {
            this.eval = eval;
        }
        
        public Task<object> Evaluate(IDictionary<string, object> vars)
        {
            return this.eval(vars);
        }

        public Task<object> GetParseTree()
        {
            throw new NotImplementedException();
        }
    }
}
