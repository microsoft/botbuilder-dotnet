// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Helper Bot Controller for ASP.NET 
    /// </summary>
    public class BotController : ApiController
    {
        protected BotFrameworkAdapter Adapter;

        public BotController(BotFrameworkAdapter adapter)
        {
            this.Adapter = adapter;
        }

        public virtual Task OnReceiveActivity(IBotContext context)
        {
            return Task.CompletedTask;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            try
            {
                await Adapter.ProcessActivty(this.Request.Headers.Authorization?.Parameter, activity, OnReceiveActivity);
                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (UnauthorizedAccessException e)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, e.Message);
            }
            catch (InvalidOperationException e)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, e.Message);
            }
        }
    }
}
