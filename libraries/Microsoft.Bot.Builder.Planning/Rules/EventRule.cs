using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Planning.Rules
{
    public class EventRule : IPlanningRule
    {
        public List<string> Events { get; set; }

        public List<IDialog> Steps { get; set; }

        public PlanChangeTypes ChangeType { get; set; }

        public EventRule(List<string> events = null, List<IDialog> steps = null, PlanChangeTypes changeType = PlanChangeTypes.DoSteps)
        {
            this.Events = events ?? new List<string>();
            this.Steps = steps ?? new List<IDialog>();
            this.ChangeType = changeType;
        }

        public async Task<List<PlanChangeList>> EvaluateAsync(PlanningContext planning, DialogEvent dialogEvent)
        {
            // Limit evaluation to only supported events
            if (Events.Contains(dialogEvent.Name))
            {
                return await OnEvaluateAsync(planning, dialogEvent).ConfigureAwait(false);
            }
            else
            {
                return null;
            }
        }

        protected virtual async Task<List<PlanChangeList>> OnEvaluateAsync(PlanningContext planning, DialogEvent dialogEvent)
        {
            if (await this.OnIsTriggeredAsync(planning, dialogEvent).ConfigureAwait(false))
            {
                return new List<PlanChangeList>()
                {
                    this.OnCreateChangeList(planning, dialogEvent)
                };
            }

            return new List<PlanChangeList>();
        }

        protected virtual async Task<bool> OnIsTriggeredAsync(PlanningContext planning, DialogEvent dialogEvent)
        {
            return true;
        }

        protected virtual PlanChangeList OnCreateChangeList(PlanningContext planning, object dialogOptions = null)
        {
            var changeList = new PlanChangeList()
            {
                ChangeType = this.ChangeType,
                Steps = new List<PlanStepState>()
            };

            Steps.ForEach(s =>
            {
                var stepState = new PlanStepState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id
                };

                if (dialogOptions != null)
                {
                    stepState.Options = dialogOptions;
                }

                changeList.Steps.Add(stepState);
            });

            return changeList;
        }
    }
}
