// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Runtime.Builders.Middleware;

namespace Microsoft.Bot.Runtime.Settings
{
    public class CoreBotAdapterOptions
    {
        public IList<IMiddlewareBuilder> Middleware { get; } = new List<IMiddlewareBuilder>();
    }
}
