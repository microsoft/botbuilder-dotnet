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
    public class UtteranceRecognizeRule : EventRule
    {
        public UtteranceRecognizeRule(string intent = null, List<string> entities = null, List<IDialog> steps = null, PlanChangeTypes changeType = PlanChangeTypes.DoSteps, string constraint = null)
            : base(events: new List<string>()
            {
                PlanningEvents.UtteranceRecognized.ToString()
            },
            steps: steps,
            changeType: changeType,
            constraint: constraint)
        {
            Intent = intent ?? null;
            Entities = entities ?? new List<string>();
        }


        /// <summary>
        /// Intent to match on
        /// </summary>
        public string Intent { get; set; }

        /// <summary>
        /// Entities which must be recognized for this rule to trigger
        /// </summary>
        public List<string> Entities { get; set; }

        protected override void GatherConstraints(List<string> constraints)
        {
            base.GatherConstraints(constraints);

            // add constraints for the intents property
            //if (!String.IsNullOrEmpty(this.Intent))
            //{
            //    constraints.Add($"dialog.DialogEvent.Value.Intents.Count > 0 && dialog.DialogEvent.Value.Intents[0] == '{this.Intent}'");
            //}

            //foreach (var entity in this.Entities)
            //{
            //    constraints.Add($"CONTAINS(DialogEvent.Entities, '{entity}')");
            //}
        }


        public override IExpression GetExpressionEval(PlanningContext planningContext, DialogEvent dialogEvent)
        {
            return new AndExpressions(
                base.GetExpressionEval(planningContext, dialogEvent), 
                new FunctionExpression(async (vars) =>
                {
                    var recognizerResult = dialogEvent.Value as RecognizerResult;
                    if (recognizerResult != null && recognizerResult.Intents.TryGetValue(this.Intent, out IntentScore score))
                    {
                        return true;
                    }
                    return false;
                })
            );
        }



        protected override PlanChangeList OnCreateChangeList(PlanningContext planning, object dialogOptions = null)
        {
            return new PlanChangeList()
            {
                ChangeType = this.ChangeType,
                Steps = Steps.Select(s => new PlanStepState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id,
                    Options = dialogOptions
                }).ToList()
            };
        }
    }
}
