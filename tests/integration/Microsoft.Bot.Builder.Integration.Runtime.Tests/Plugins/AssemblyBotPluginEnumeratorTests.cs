// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Bot.Builder.Integration.Runtime.Plugins;
using Microsoft.Bot.Builder.Runtime.Plugins;
using Microsoft.Bot.Builder.Runtime.Tests.Plugins.Implementations;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Plugins
{
    public class AssemblyBotPluginEnumeratorTests
    {
        [Fact]
        public void Constructor_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                "loadContext",
                () => new AssemblyBotPluginEnumerator(loadContext: null));
        }

        [Theory]
        [InlineData((string)null)]
        [InlineData("")]
        public void GetPlugins_Throws_ArgumentNullException(string pluginName)
        {
            var enumerator = new AssemblyBotPluginEnumerator(AssemblyLoadContext.Default);

            Assert.Throws<ArgumentNullException>(
                "pluginName",
                () => enumerator.GetPlugins(pluginName).ToList());
        }

        [Fact]
        public void GetPlugins_Throws_AssemblyNotFound()
        {
            var enumerator = new AssemblyBotPluginEnumerator(AssemblyLoadContext.Default);

            Assert.Throws<FileNotFoundException>(
                () => enumerator.GetPlugins(pluginName: "Fake.Assembly.Name").ToList());
        }

        [Fact]
        public void GetPlugins_Throws_InvalidAssemblyName()
        {
            var enumerator = new AssemblyBotPluginEnumerator(AssemblyLoadContext.Default);

            Assert.Throws<FileLoadException>(
                () => enumerator.GetPlugins(pluginName: "*//\\-_+!?#$").ToList());
        }

        [Fact]
        public void GetPlugins_Succeeds()
        {
            var enumerator = new AssemblyBotPluginEnumerator(AssemblyLoadContext.Default);

            IList<IBotPlugin> plugins = enumerator.GetPlugins(Assembly.GetExecutingAssembly().FullName).ToList();

            Assert.Equal(3, plugins.Count);
            Assert.Contains(plugins, p => typeof(PublicBotPlugin) == p.GetType());
            Assert.Contains(plugins, p => typeof(AdventureWorksAdapterPlugin) == p.GetType());
            Assert.Contains(plugins, p => typeof(ContosoAdapterPlugin) == p.GetType());
        }

        // The below type definitions are intended purely for test usage to ensure that the assembly
        // contains protected and private implementations of IBotPlugin to ensure that they are not
        // returned by the enumerator. To avoid compilation errors, these must be defined as nested
        // class definitions.

        protected class ProtectedBotPlugin : IBotPlugin
        {
            public void Load(IBotPluginLoadContext context)
            {
            }
        }

        private class PrivateBotPlugin : IBotPlugin
        {
            public void Load(IBotPluginLoadContext context)
            {
            }
        }
    }
}
