// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace SimpleRootBot.Middleware
{
    /// <summary>
    /// A dummy middleware to test skills behavior with middleware.
    /// </summary>
    public class DummyMiddleware : IMiddleware
    {
        private readonly string _label;

        public DummyMiddleware(string label)
        {
            _label = label;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            var message = $"{_label} {turnContext.Activity.Type} {turnContext.Activity.Text}";
            Console.WriteLine(message);

            // Register outgoing handler.
            turnContext.OnSendActivities(OutgoingHandler);

            // Continue processing messages.
            await next(cancellationToken);
        }

        private async Task<ResourceResponse[]> OutgoingHandler(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            // PVA requirements:
            // how do I get the BotId and the SkillId here? 
            // How do I know if this outgoing request is coming from a skill or the host?
            foreach (var activity in activities)
            {
                var message = $"{_label} {activity.Type} {activity.Text}";
                Console.WriteLine(message);
            }

            return await next();
        }
    }
}
