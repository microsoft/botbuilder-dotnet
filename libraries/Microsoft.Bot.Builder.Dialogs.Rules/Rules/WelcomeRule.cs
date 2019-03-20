using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative.Expressions;
using Microsoft.Bot.Builder.Dialogs.Expressions;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Rules
{
    public class WelcomeRule : EventRule
    {
        private const string welcomeProperty = "conversation.welcomed";

        public string WelcomeProperty { get; set; }

        public WelcomeRule(List<IDialog> steps = null, string conversationProperty = welcomeProperty, string constraint = null)
            : base(events: new List<string>()
                {
                    PlanningEvents.ActivityReceived.ToString(),
                    PlanningEvents.PlanStarted.ToString(),
                    PlanningEvents.Fallback.ToString(),
                },
                  steps: steps,
                  changeType: PlanChangeTypes.DoSteps,
                  constraint: constraint)
        {
            this.WelcomeProperty = conversationProperty;
        }

        public override IExpression GetExpressionEval(PlanningContext planningContext, DialogEvent dialogEvent)
        {
            return new AndExpressions(
                base.GetExpressionEval(planningContext, dialogEvent),
                new FunctionExpression(async (vars) =>
                   {
                       // Have we already welcomed the user?
                       if (planningContext.State.GetValue<bool>(welcomeProperty))
                       {
                           // don't trigger
                           return false;
                       }

                       // inspect activity and decide if we should fire
                       if (dialogEvent.Name == PlanningEvents.ActivityReceived.ToString())
                       {
                           // Filter to only ConversationUpdate activities
                           var activity = planningContext.Context.Activity;

                           if (activity.Type == ActivityTypes.ConversationUpdate && activity.MembersAdded?.Count > 0)
                           {
                               foreach (var member in activity.MembersAdded)
                               {
                                   if (member.Id != activity.Recipient.Id)
                                   {
                                       return true;
                                   }
                               }
                           }

                           return false;
                       }
                       else if (dialogEvent.Name == PlanningEvents.PlanStarted.ToString())
                       {
                           // Trigger the greeting
                           return true;
                       }

                       return false;
                   })
            );

        }

        public override Task<List<PlanChangeList>> OnExecuteAsync(PlanningContext planning)
        {
            // set that we have executed this
            // BUGBUG: currently this is getting set even if we don't win the consultation round.
            //         This should be changed to a step that sets the property as part of the plan
            //         change made.  Just insert the new step at the begining of the steps for the plan 
            //         change returned.
            planning.State.SetValue(welcomeProperty, true);

            // add steps for the rule to the plan
            return base.OnExecuteAsync(planning);
        }
    }
}
