using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Expressions
{
    public class CommonExpression : IExpressionEval
    {
        private string _expression;
        private Term _parseTree;

        public CommonExpression() { }
        public CommonExpression(string condition)
        {
            this.Expression = condition;
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
                try
                {
                    var result = ExpressionEngine.Evaluate(this._parseTree, state);
                    return result;
                }
                catch (Exception err)
                {
                    Console.WriteLine(string.Join(Environment.NewLine, err.Message));

                    // TODO WHAT TO THROW?
                    throw;
                }
            }

            throw new ArgumentNullException(nameof(Expression));
        }

        public Task<object> Evaluate(string expression, IDictionary<string, object> vars)
        {
            var parseTree = ExpressionEngine.Parse(expression);
            var result = ExpressionEngine.Evaluate(this._parseTree, vars);
            return Task.FromResult(result);
        }
    }
}
