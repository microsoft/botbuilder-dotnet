using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative.Expressions;
using Microsoft.Bot.Builder.Dialogs.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Rules
{
    public class EventRule : Rule
    {
        public EventRule(List<string> events = null, List<IDialog> steps = null, PlanChangeTypes changeType = PlanChangeTypes.DoSteps, string constraint = null)
            : base(constraint: constraint, steps: steps, changeType: changeType)
        {
            this.Events = events ?? new List<string>();
            this.Steps = steps ?? new List<IDialog>();
            this.ChangeType = changeType;
        }

        public List<string> Events { get; set; }

        protected override void GatherConstraints(List<string> constraints)
        {
            base.GatherConstraints(constraints);

            //// add in the constraints for Events property
            //StringBuilder sb = new StringBuilder();
            //string append = string.Empty;
            //foreach (var evt in Events)
            //{
            //    sb.Append($"{append} dialog.DialogEvent.Name == '{evt}' ");
            //    append = "||";
            //}
            //constraints.Add(sb.ToString());
        }

        public override IExpression GetExpressionEval(PlanningContext planningContext, DialogEvent dialogEvent)
        {
            var baseExpression = base.GetExpressionEval(planningContext, dialogEvent);

            return new FunctionExpression(async (vars) =>
            {
                if (baseExpression != null)
                {
                    var result = (bool)await baseExpression.Evaluate(vars);
                    if (result == false)
                    {
                        return false;
                    }
                }

                foreach(var evt in this.Events)
                {
                    if (dialogEvent.Name == evt)
                    {
                        return true;
                    }
                }
                return false;
            });

        }

    }
}
