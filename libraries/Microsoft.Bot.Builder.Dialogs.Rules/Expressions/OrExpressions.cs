using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Expressions
{
    public class OrExpressions  : IExpression
    {
        private IExpression expression1;
        private IExpression expression2;

        internal OrExpressions(IExpression expression1, IExpression expression2)
        {
            this.expression1 = expression1;
            this.expression2 = expression2;
        }
        
        public async Task<object> Evaluate(IDictionary<string, object> vars)
        {
            bool result1 = false;
            bool result2 = false;

            if (this.expression1 != null)
            {
                result1 = (bool)await this.expression1.Evaluate(vars);
            }

            if (this.expression2 != null)
            {
                result2 = (bool)await this.expression2.Evaluate(vars);
            }

            return result1 || result2;
        }

        public Task<object> GetParseTree()
        {
            throw new NotImplementedException();
        }
    }
}
