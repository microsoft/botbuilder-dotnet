// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Integration.Runtime.Plugins;
using Microsoft.Bot.Builder.Runtime.Plugins;

namespace Microsoft.Bot.Builder.Runtime.Tests.Plugins
{
    public class TestBotPluginEnumerator : IBotPluginEnumerator
    {
        private readonly IDictionary<string, ICollection<IBotPlugin>> _plugins;

        public TestBotPluginEnumerator(IDictionary<string, ICollection<IBotPlugin>> plugins = null)
        {
            _plugins = plugins ?? new Dictionary<string, ICollection<IBotPlugin>>(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<IBotPlugin> GetPlugins(string pluginName)
        {
            if (string.IsNullOrEmpty(pluginName))
            {
                throw new ArgumentNullException(nameof(pluginName));
            }

            if (!_plugins.TryGetValue(pluginName, out ICollection<IBotPlugin> matchingPlugins))
            {
                yield break;
            }

            foreach (IBotPlugin plugin in matchingPlugins)
            {
                yield return plugin;
            }
        }
    }
}
