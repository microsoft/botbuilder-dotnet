// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Runtime.Plugins;

namespace Microsoft.Bot.Builder.Runtime.Tests.Plugins
{
    public class TestBotPlugin : IBotPlugin
    {
        private readonly Action<IBotPluginLoadContext> _loadAction;

        public TestBotPlugin(Action<IBotPluginLoadContext> loadAction)
        {
            _loadAction = loadAction ?? throw new ArgumentNullException(nameof(loadAction));
        }

        public void Load(IBotPluginLoadContext context)
        {
            _loadAction(context);
        }
    }
}
