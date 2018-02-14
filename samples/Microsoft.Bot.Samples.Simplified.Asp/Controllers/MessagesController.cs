// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Samples.Simplified.Asp.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : BotController
    {
        public MessagesController(IConfiguration configuration)
            : base(configuration)
        {
        }

        protected override Task<List<IActivity>> Receive(IMessageActivity activity)
        {
            return Task.FromResult(new List<IActivity> { new Activity { Text = $"echo: {activity.Text}" } });
        }
    }
}
