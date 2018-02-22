// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Schema;

namespace AlarmBot
{
    /// <summary>
    /// Helper controller for boiler plate adapter usage
    /// </summary>
    public class BotController : Controller
    {
        BotFrameworkAdapter adapter;

        public BotController(BotFrameworkAdapter adapter)
        {
            this.adapter = adapter;
        }

        public virtual Task OnReceiveActivity(IBotContext context)
        {
            return Task.CompletedTask;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await adapter.ProcessActivty(this.Request.Headers["Authorization"].FirstOrDefault(), activity, OnReceiveActivity);
                return this.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return this.Unauthorized();
            }
            catch (InvalidOperationException e)
            {
                return this.NotFound(e.Message);
            }
        }
    }
}
