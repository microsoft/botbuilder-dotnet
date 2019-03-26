using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

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

            // add in the constraints for Events property
            StringBuilder sb = new StringBuilder();
            string append = string.Empty;
            foreach (var evt in Events)
            {
                sb.Append($"{append} (turn.DialogEvent.Name == '{evt}') ");
                append = "||";
            }
            constraints.Add(sb.ToString());
        }

    }
}
