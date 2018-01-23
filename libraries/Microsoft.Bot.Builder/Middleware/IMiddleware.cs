// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Middleware.MiddlewareSet;

namespace Microsoft.Bot.Builder.Middleware
{
    public interface IMiddleware { }

    public interface IContextCreated : IMiddleware
    {
        Task ContextCreated(IBotContext context, NextDelegate next);
    }

    public interface IReceiveActivity : IMiddleware
    {
        Task ReceiveActivity(IBotContext context, NextDelegate next);
    }

    public interface IPostActivity : IMiddleware
    {
        Task PostActivity(IBotContext context, IList<IActivity> activities, NextDelegate next);
    }

    public class AnonymousReceiveMiddleware : IReceiveActivity
    {
        private readonly Func<IBotContext, NextDelegate, Task> _toCall;

        public AnonymousReceiveMiddleware(Func<IBotContext, NextDelegate, Task> anonymousMethod)
        {
            _toCall = anonymousMethod ?? throw new ArgumentNullException(nameof(anonymousMethod));
        }

        public Task ReceiveActivity(IBotContext context, NextDelegate next)
        {
            return _toCall(context, next);
        }
    }

    public class AnonymousContextCreatedMiddleware : IContextCreated
    {
        private readonly Func<IBotContext, NextDelegate, Task> _toCall;

        public AnonymousContextCreatedMiddleware(Func<IBotContext, NextDelegate, Task> anonymousMethod)
        {
            _toCall = anonymousMethod ?? throw new ArgumentNullException(nameof(anonymousMethod));
        }

        public Task ContextCreated(IBotContext context, NextDelegate next)
        {
            return _toCall(context, next);
        }
    }

    public class AnonymousPostActivityMiddleware : IPostActivity
    {
        private readonly Func<IBotContext, IList<IActivity>, NextDelegate, Task> _toCall;

        public AnonymousPostActivityMiddleware(Func<IBotContext, IList<IActivity>, NextDelegate, Task> anonymousMethod)
        {
            _toCall = anonymousMethod ?? throw new ArgumentNullException(nameof(anonymousMethod));
        }

        public Task PostActivity(IBotContext context, IList<IActivity> activities, NextDelegate next)
        {
            return _toCall(context, activities, next);
        }
    }
}
