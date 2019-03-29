//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Bot.Builder.Dialogs;
//using Microsoft.Bot.Builder.Dialogs.Expressions;
//using Microsoft.Bot.Builder.Dialogs.Rules.Expressions;

//namespace Microsoft.Bot.Builder.Dialogs.Rules.Rules
//{
//    public class IfConditionRuleCondition
//    {
//        public IExpressionEval Expression { get; set; }
//        public List<IRule> Rules { get; set; }

//    }

//    public class IfConditionRule : IRule
//    {
//        public List<IfConditionRuleCondition> Conditionals { get; set; }

//        public List<IDialog> Steps
//        {
//            get
//            {
//                var steps = new List<IDialog>();
//                Conditionals.ForEach(c => c.Rules.ForEach(r => r.Steps.ForEach(s => steps.Add(s))));
//                return steps;
//            }
//        }


//        public IfConditionRule(IExpressionEval expression, List<IRule> rules)
//        {
//            if (expression != null && rules != null)
//            {
//                Conditionals.Add(new IfConditionRuleCondition()
//                {
//                    Expression = expression,
//                    Rules = rules
//                });
//            }
//        }

//        public IfConditionRule()
//        {
//        }

//        public async Task<List<PlanChangeList>> ExecuteAsync(PlanningContext planning)
//        {
//            // Find first matching conditional
//            var changes = new List<PlanChangeList>();

//            for (int i = 0; i < Conditionals.Count; i++)
//            {
//                var conditional = Conditionals[i];

//                var result = await conditional.Expression.Evaluate(planning.State).ConfigureAwait(false);
//                if ((bool)result)
//                {
//                    // Evaluate child rules
//                    for (int j = 0; j < conditional.Rules.Count; j++)
//                    {
//                        var change = await conditional.Rules[j].ExecuteAsync(planning).ConfigureAwait(false);

//                        if (change != null)
//                        {
//                            changes.AddRange(change);
//                        }
//                    }

//                    break;
//                }
//            }

//            return changes.Count > 0 ? changes : null;
//        }

//        public void ElseIf(IExpressionEval expression, List<IRule> rules)
//        {
//            Conditionals.Add(new IfConditionRuleCondition()
//            {
//                Expression = expression,
//                Rules = rules
//            });
//        }

//        public void Else(List<IRule> rules)
//        {
//            Conditionals.Add(new IfConditionRuleCondition()
//            {
//                Expression = new StaticExpression(true),
//                Rules = rules
//            });
//        }

//        public IExpressionEval GetExpression(PlanningContext context)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
