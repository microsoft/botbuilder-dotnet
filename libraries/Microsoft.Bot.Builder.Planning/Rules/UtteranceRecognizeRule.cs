using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Planning.Rules
{
    public class UtteranceRecognizeRule : EventRule
    {
        public string Intent { get; set; }

        public List<string> Entities { get; set; }

        public UtteranceRecognizeRule(string intent = null, List<string> entities = null, List<IDialog> steps = null, PlanChangeTypes changeType = PlanChangeTypes.DoSteps)
            : base(new List<string>()
            {
                PlanningEvents.UtteranceRecognized.ToString()
            }, 
            steps, 
            changeType)
        {
            Intent = intent ?? null;
            Entities = entities ?? new List<string>();
        }        

        protected override async Task<bool> OnIsTriggeredAsync(PlanningContext planning, DialogEvent dialogEvent)
        {
            if (dialogEvent.Value is RecognizerResult recognizerResult)
            {
                // Ensure all intents recognized
                if (recognizerResult.Intents == null)
                {
                    return false;
                }

                if (!recognizerResult.Intents.Any(r => r.Key == Intent))
                {
                    return false;
                }
                
                // TODO: Ensure all entities recognized
            }

            return true;
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
