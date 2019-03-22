using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Expressions;
using Microsoft.Bot.Builder.Dialogs.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Rules
{
    public class IfPropertyRuleCondition
    {
        public IExpression Expression { get; set; }
        public List<IRule> Rules { get; set; }

    }

    public class IfPropertyRule : IRule
    {
        public List<IfPropertyRuleCondition> Conditionals { get; set; }

        public List<IDialog> Steps
        {
            get
            {
                var steps = new List<IDialog>();
                Conditionals.ForEach(c => c.Rules.ForEach(r => r.Steps.ForEach(s => steps.Add(s))));
                return steps;
            }
        }


        public IfPropertyRule(IExpression expression, List<IRule> rules)
        {
            if (expression != null && rules != null)
            {
                Conditionals.Add(new IfPropertyRuleCondition()
                {
                    Expression = expression,
                    Rules = rules
                });
            }
        }

        public IfPropertyRule()
        {
        }

        public async Task<List<PlanChangeList>> EvaluateAsync(PlanningContext planning, DialogEvent dialogEvent)
        {
            // Find first matching conditional
            var changes = new List<PlanChangeList>();

            for (int i = 0; i < Conditionals.Count; i++)
            {
                var conditional = Conditionals[i];

                var result = await conditional.Expression.Parse.Evaluate(planning.State).ConfigureAwait(false);
                if ((bool)result)
                {
                    // Evaluate child rules
                    for (int j = 0; j < conditional.Rules.Count; j++)
                    {
                        var change = await conditional.Rules[j].EvaluateAsync(planning, dialogEvent).ConfigureAwait(false);

                        if (change != null)
                        {
                            changes.AddRange(change);
                        }
                    }

                    break;
                }
            }

            return changes.Count > 0 ? changes : null;
        }

        public void ElseIf(IExpression expression, List<IRule> rules)
        {
            Conditionals.Add(new IfPropertyRuleCondition()
            {
                Expression = expression,
                Rules = rules
            });
        }

        public void Else(List<IRule> rules)
        {
            Conditionals.Add(new IfPropertyRuleCondition()
            {
                Expression = new StaticExpression(true),
                Rules = rules
            });
        }

        internal class StaticExpression : IExpression
        {
            private bool result;

            public string Expression { get { return result.ToString();  } }

            public Expression Parse { get { return new Constant(result); } }

            internal StaticExpression(bool result)
            {
                this.result = result;
            }
        }
    }
}
