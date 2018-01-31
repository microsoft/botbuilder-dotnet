// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    public class TrustServiceUrlAttribute : ActionFilterAttribute
    {
        private readonly ICredentialProvider Options;

        public TrustServiceUrlAttribute(ICredentialProvider options)
        {
            this.Options = options;
        }

        public async override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!await Options.IsAuthenticationDisabledAsync())
            {
                var activities = GetActivities(context);

                foreach (var activity in activities)
                {
                    MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);
                }
            }
            await next();
        }

        public static IList<Activity> GetActivities(ActionExecutingContext actionContext)
        {
            var activties = actionContext.ActionArguments.Select(t => t.Value).OfType<Activity>().ToList();
            if (activties.Any())
            {
                return activties;
            }
            else
            {
                var objects =
                    actionContext.ActionArguments.Where(t => t.Value is JObject || t.Value is JArray)
                        .Select(t => t.Value).ToArray();
                if (objects.Any())
                {
                    activties = new List<Activity>();
                    foreach (var obj in objects)
                    {
                        activties.AddRange((obj is JObject) ? new Activity[] { ((JObject)obj).ToObject<Activity>() } : ((JArray)obj).ToObject<Activity[]>());
                    }
                }
            }
            return activties;
        }
    }
}