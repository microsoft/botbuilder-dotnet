using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Expressions;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Expressions
{
    public class CommonExpression: IExpression
    {
        private string _expression;

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
                this.Parse = new ExpressionEngine().Parse(value);
            }
        }

        public Expression Parse { get; private set; }
    }
}
