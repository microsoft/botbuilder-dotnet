using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Rules
{
    public class BeginDialogRule : EventRule
    {
        /// <summary>
        /// Rule which fires when the dialog is started.
        /// </summary>
        /// <param name="steps"></param>
        /// <param name="changeType"></param>
        /// <param name="constraint"></param>
        public BeginDialogRule(List<IDialog> steps = null, PlanChangeTypes changeType = PlanChangeTypes.DoSteps, string constraint = null)
            : base(events: new List<string>()
            {
                PlanningEvents.BeginDialog.ToString()
            },
            steps: steps,
            changeType: changeType,
            constraint: constraint)
        {
        }
    }
}
