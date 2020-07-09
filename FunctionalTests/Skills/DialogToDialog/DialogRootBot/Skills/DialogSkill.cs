// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.DialogRootBot.Skills
{
    public class DialogSkill : SkillDefinition
    {
        private const string SkillActionBookFlight = "BookFlight";
        private const string SkillActionBookFlightWithInputParameters = "BookFlight with input parameters";
        private const string SkillActionGetWeather = "GetWeather";
        private const string SkillActionOAuthTest = "OAuthTest";
        private const string SkillActionEchoSkillBot = "EchoSkill (Root->DialogSkill->EchoSkill)";
        private const string SkillActionMessage = "Message (sends 'Book a flight' as message)";

        public override IList<string> GetActions()
        {
            return new List<string>
            {
                SkillActionBookFlight,
                SkillActionBookFlightWithInputParameters,
                SkillActionGetWeather,
                SkillActionOAuthTest,
                SkillActionEchoSkillBot,
                SkillActionMessage
            };
        }

        public override Activity CreateBeginActivity(string actionId)
        {
            Activity activity;

            // Send an event activity to the skill with "BookFlight" in the name.
            if (actionId.Equals(SkillActionBookFlight, StringComparison.CurrentCultureIgnoreCase))
            {
                activity = (Activity)Activity.CreateEventActivity();
                activity.Name = SkillActionBookFlight;
                return activity;
            }

            // Send an event activity to the skill with "BookFlight" in the name and some testing values.
            if (actionId.Equals(SkillActionBookFlightWithInputParameters, StringComparison.CurrentCultureIgnoreCase))
            {
                activity = (Activity)Activity.CreateEventActivity();
                activity.Name = SkillActionBookFlight;
                activity.Value = JObject.Parse("{ \"origin\": \"New York\", \"destination\": \"Seattle\"}");
                return activity;
            }

            // Send an event activity to the skill with "GetWeather" in the name and some testing values.
            if (actionId.Equals(SkillActionGetWeather, StringComparison.CurrentCultureIgnoreCase))
            {
                activity = (Activity)Activity.CreateEventActivity();
                activity.Name = SkillActionGetWeather;
                activity.Value = JObject.Parse("{ \"latitude\": 47.614891, \"longitude\": -122.195801}");
                return activity;
            }

            // Send an event activity to the skill with "OAuthTest" in the name.
            if (actionId.Equals(SkillActionOAuthTest, StringComparison.CurrentCultureIgnoreCase))
            {
                activity = (Activity)Activity.CreateEventActivity();
                activity.Name = SkillActionOAuthTest;
                return activity;
            }

            // Send an event activity to the skill with "EchoSkillBot" in the name.
            if (actionId.Equals(SkillActionEchoSkillBot, StringComparison.CurrentCultureIgnoreCase))
            {
                activity = (Activity)Activity.CreateEventActivity();
                activity.Name = "EchoSkill";
                return activity;
            }

            if (actionId.Equals(SkillActionMessage, StringComparison.CurrentCultureIgnoreCase))
            {
                activity = (Activity)Activity.CreateMessageActivity();
                activity.Name = SkillActionMessage;
                activity.Text = "Book a flight";
                return activity;
            }

            throw new InvalidOperationException($"Unable to create begin activity for \"{actionId}\".");
        }
    }
}
