using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using Microsoft.Bot.Builder.Dialogs.Expressions;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Expressions
{

    public class CommonExpressionFactory : IExpressionFactory
    {
        public IExpression CreateExpression(string expression)
        {
            return new CommonExpression(expression);
        }
    }

    public class CommonExpression : IExpression
    {
        private string _expression;
        private IParseTree _parseTree;

        public CommonExpression() { }
        public CommonExpression(string expression)
        {
            this.Expression = expression;
        }

        public string Expression
        {
            get { return _expression; }
            set
            {
                this._expression = value;
                this._parseTree = ExpressionEngine.Parse(value);
            }
        }


        public async Task<object> Evaluate(IDictionary<string, object> state)
        {
            if (this._parseTree != null)
            {
                var result = ExpressionEngine.Evaluate(this._parseTree, state);
                return result;
            }

            throw new ArgumentNullException(nameof(Expression));
        }


        public Task<object> GetParseTree()
        {
            return Task.FromResult((object)this._parseTree);
        }

    }
}
