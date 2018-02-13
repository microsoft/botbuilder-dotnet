// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Bot.Builder
{
    public class Bot : Middleware.MiddlewareSet
    {
        private ActivityAdapterBase _adapter;

        public new Bot OnReceive(Func<IBotContext, NextDelegate, Task> anonymousMethod)
        {
            base.OnReceive(anonymousMethod);
            return this;
        }

        public Bot(ActivityAdapterBase adapter) : base()
        {
            BotAssert.AdapterNotNull(adapter);
            _adapter = adapter;

            // Hook up the Adapter so that incoming data is routed 
            // through the Middleware Pipeline
            _adapter.OnReceive = this.RunPipeline;

            this.Use(new Middleware.SendToAdapterMiddleware(this));
            this.Use(new Middleware.TemplateManager());
        }

        public new Bot Use(Middleware.IMiddleware middleware)
        {
            base.Use(middleware);
            return this;
        }

        public ActivityAdapterBase Adapter { get => _adapter; }

        private async Task RunPipeline(IBotContext context, Func<IBotContext, Task> proactiveCallback = null)
        {
            BotAssert.ContextNotNull(context);

            System.Diagnostics.Trace.TraceInformation($"Middleware: Beginning Pipeline for {context.ConversationReference.ActivityId}");

            // Call any registered Middleware Components looking for ContextCreated()
            await this.ContextCreated(context).ConfigureAwait(false);

            // Call any registered Middleware Components looking for ReceiveActivity()
            if (context.Request != null)
                await this.ReceiveActivity(context).ConfigureAwait(false);

            // call back to caller
            if (proactiveCallback != null)
                await proactiveCallback(context).ConfigureAwait(false);

            // Call any registered Middleware Components looking for SendActivity()
            if (context.Responses != null && context.Responses.Any())
                await this.SendActivity(context, context.Responses).ConfigureAwait(false);

            System.Diagnostics.Trace.TraceInformation($"Middleware: Ending Pipeline for {context.ConversationReference.ActivityId}");
        }

        public async Task RunPipeline(IActivity activity, IDictionary<string, StringValues> requestInfo)
        {
            BotAssert.ActivityNotNull(activity);

            var context = new BotContext(this, activity, requestInfo);

            await RunPipeline(context).ConfigureAwait(false);
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
