using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Planning.Rules
{
    public class WelcomeRule : EventRule
    {
        private const string welcomeProperty = "conversation.welcomed";

        public string WelcomeProperty { get; set; }

        public WelcomeRule()
            : base(new List<string>()
            {
                PlanningEvents.ActivityReceived.ToString(),
                PlanningEvents.PlanStarted.ToString(),
                PlanningEvents.Fallback.ToString(),
            })
        {
        }

        public WelcomeRule(List<IDialog> steps = null, string conversationProperty = welcomeProperty)
            : base(new List<string>()
            {
                PlanningEvents.ActivityReceived.ToString(),
                PlanningEvents.PlanStarted.ToString(),
                PlanningEvents.Fallback.ToString(),
            }, steps, PlanChangeTypes.DoSteps)
        {
            this.WelcomeProperty = conversationProperty;
        }

        protected override async Task<bool> OnIsTriggeredAsync(PlanningContext planning, DialogEvent dialogEvent)
        {
            if (dialogEvent.Name == PlanningEvents.ActivityReceived.ToString())
            {
                return HandleActivityReceived(planning);
            }
            else if (dialogEvent.Name == PlanningEvents.PlanStarted.ToString())
            {
                return HandlePlanStarted(planning);
            }

            return false;
        }

        private bool HandleActivityReceived(PlanningContext planning)
        {
            // Filter to only ConversationUpdate activities
            var activity = planning.Context.Activity;

            if (activity.Type == ActivityTypes.ConversationUpdate && activity.MembersAdded?.Count > 0)
            {
                // Have we already welcomed the user?
                if (!planning.State.GetValue<bool>(welcomeProperty))
                {
                    // Ensure a user is being added
                    var userAdded = false;

                    foreach (var member in activity.MembersAdded)
                    {
                        if (member.Id != activity.Recipient.Id)
                        {
                            userAdded = true;
                        }
                    }

                    // Trigger only if user added
                    if (userAdded)
                    {
                        planning.State.SetValue(welcomeProperty, true);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HandlePlanStarted(PlanningContext planning)
        {
            // Have we already welcomed the user?
            if (!planning.State.GetValue<bool>(welcomeProperty))
            {
                // Trigger the greeting
                planning.State.SetValue(welcomeProperty, true);
                return true;
            }

            return false;
        }
    }
}
