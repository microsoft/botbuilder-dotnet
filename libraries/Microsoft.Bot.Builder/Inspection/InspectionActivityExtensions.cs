// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    public static class InspectionActivityExtensions
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

        public static IEnumerable<Activity> Clone(this List<Activity> activities)
        {
            return activities.Select(activity => activity.Clone());
        }

        public static Activity Clone(this Activity activity)
        {
            return JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity, jsonSerializerSettings));
        }

        public static Activity CreateTraceActivity(this BotState state, ITurnContext turnContext)
        {
            var name = state.GetType().Name;
            var cachedState = turnContext.TurnState.Get<object>(name);
            var obj = JObject.FromObject(cachedState)["State"];
            return (Activity)Activity.CreateTraceActivity("Interception", value: obj);
        }
    }
}
