// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    public class Bot
    {
        private ActivityAdapterBase _adapter;
        private readonly Middleware.MiddlewareSet _middlewareSet = new Middleware.MiddlewareSet();
        Func<IBotContext, Task> _onReceive = null;

        public void OnReceive(Func<IBotContext, Task> anonymousMethod)
        {
            _onReceive = anonymousMethod;            
        }

        public Bot(ActivityAdapterBase adapter) : base()
        {
            BotAssert.AdapterNotNull(adapter);
            _adapter = adapter;

            // Hook up the Adapter so that incoming data is routed 
            // through the Middleware Pipeline
            _adapter.OnReceive = this.RunPipeline;

            this.Use(new Middleware.BindOutoingResponsesMiddlware()); 
            this.Use(new Middleware.SendToAdapterMiddleware(this));
            this.Use(new Middleware.TemplateManager());
        }

        public Bot Use(Middleware.IMiddleware middleware)
        {
            _middlewareSet.Use(middleware);
            return this;
        }

        public ActivityAdapterBase Adapter { get => _adapter; }

        private async Task RunPipeline(IBotContext context, Func<IBotContext, Task> proactiveCallback = null)
        {
            BotAssert.ContextNotNull(context);

            System.Diagnostics.Trace.TraceInformation($"Middleware: Beginning Pipeline for {context.ConversationReference.ActivityId}");

            // Call any registered Middleware Components looking for ContextCreated()
            await _middlewareSet.ContextCreated(context).ConfigureAwait(false);

            // Call any registered Middleware Components looking for ReceiveActivity()
            if (context.Request != null)
            {
                bool didAllMiddlewareRun = await _middlewareSet.ReceiveActivityWithStatus(context).ConfigureAwait(false);
                if (didAllMiddlewareRun)
                {
                    // If the dev has registered a Receive Handler, call it. 
                    if (_onReceive != null)
                    {
                        await _onReceive(context).ConfigureAwait(false);
                    }
                }
                else
                {
                    // One of the middleware instances did not call Next(). When this happens,
                    // by design, we do NOT call the OnReceive handler. This allows
                    // Middleware interceptors to be written that activly prevent certain
                    // Activites from being run. 
                }
            }

            // call back to caller
            if (proactiveCallback != null)
            {
                await proactiveCallback(context).ConfigureAwait(false);
            }

            // Call any registered Middleware Components looking for SendActivity()
            if (context.Responses != null && context.Responses.Any())
            {
                await _middlewareSet.SendActivity(context, context.Responses).ConfigureAwait(false);
            }

            System.Diagnostics.Trace.TraceInformation($"Middleware: Ending Pipeline for {context.ConversationReference.ActivityId}");
        }

        public async Task RunPipeline(IActivity activity)
        {
            BotAssert.ActivityNotNull(activity);

            var context = new BotContext(this, activity);

            await RunPipeline(context).ConfigureAwait(false);
        }

        public async Task SendActivity(IBotContext context, List<IActivity> activities)
        {
            await _middlewareSet.SendActivity(context, activities);
        }

    /// <summary>
    /// Create proactive context around conversation reference
    /// All middleware pipelines will be processed
    /// </summary>
    /// <param name="reference">reference to create context around</param>
    /// <param name="proactiveCallback">callback where you can continue the conversation</param>
    /// <returns>task when completed</returns>
    public virtual async Task CreateContext(ConversationReference reference, Func<IBotContext, Task> proactiveCallback)
        {
            var context = new BotContext(this, reference);
            await RunPipeline(context, proactiveCallback).ConfigureAwait(false);
        }

    }
}
