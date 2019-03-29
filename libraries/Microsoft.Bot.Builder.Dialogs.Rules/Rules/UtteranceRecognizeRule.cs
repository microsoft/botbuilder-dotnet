using System;
using System.Collections.Generic;
using System.Linq;

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
            if (!String.IsNullOrEmpty(this.Intent))
            {
                constraints.Add($"turn.DialogEvent.Value.Intents.{this.Intent}.Score > 0.5");
            }

            //foreach (var entity in this.Entities)
            //{
            //    constraints.Add($"CONTAINS(DialogEvent.Entities, '{entity}')");
            //}
        }

        protected override PlanChangeList OnCreateChangeList(PlanningContext planning, object dialogOptions = null)
        {
            var dialogEvent = planning.State.Turn["DialogEvent"] as DialogEvent;
            if (dialogEvent.Value is RecognizerResult recognizerResult)
            {
                Dictionary<string, object> entitiesRecognized = new Dictionary<string, object>();
                entitiesRecognized = recognizerResult.Entities.ToObject<Dictionary<string, object>>();

                return new PlanChangeList()
                {
                    ChangeType = this.ChangeType,
                    IntentsMatched = new List<string> {
                        this.Intent,
                    },
                    EntitiesMatched = this.Entities,
                    EntitiesRecognized = entitiesRecognized,
                    Steps = Steps.Select(s => new PlanStepState()
                    {
                        DialogStack = new List<DialogInstance>(),
                        DialogId = s.Id,
                        Options = dialogOptions
                    }).ToList()
                };
            }

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
