// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Middleware;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Integration.AspNet
{
    public class BotFrameworkOptions
    {
        private readonly List<IMiddleware> _middleware;

        public BotFrameworkOptions()
        {
            RouteBaseUrl = "bot/";
            _middleware = new List<IMiddleware>();
        }

        public string RouteBaseUrl { get; set; }
        public string AppId { get; set; }
        public string AppPassword { get; set; }
        public List<IMiddleware> Middleware { get => _middleware; }
    }
}