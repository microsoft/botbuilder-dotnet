using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Expressions
{
    public class StaticExpression : IExpression
    {
        private bool result;

        internal StaticExpression(bool result)
        {
            this.result = result;
        }

        public Task<object> Evaluate(IDictionary<string, object> vars)
        {
            return Task.FromResult((object)this.result);
        }

        public Task<object> GetParseTree()
        {
            throw new NotImplementedException();
        }
    }
}
