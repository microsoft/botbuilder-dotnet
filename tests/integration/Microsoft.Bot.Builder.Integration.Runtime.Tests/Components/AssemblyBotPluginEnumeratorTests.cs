// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Bot.Builder.Integration.Runtime.Component;
using Microsoft.Bot.Builder.Runtime.Tests.Components.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Components
{
    public class AssemblyBotPluginEnumeratorTests
    {
        [Fact]
        public void Constructor_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                "loadContext",
                () => new AssemblyBotComponentEnumerator(loadContext: null));
        }

        [Theory]
        [InlineData((string)null)]
        [InlineData("")]
        public void GetPlugins_Throws_ArgumentNullException(string pluginName)
        {
            var enumerator = new AssemblyBotComponentEnumerator(AssemblyLoadContext.Default);

            Assert.Throws<ArgumentNullException>(
                "componentName",
                () => enumerator.GetComponents(pluginName).ToList());
        }

        [Fact]
        public void GetPlugins_Throws_AssemblyNotFound()
        {
            var enumerator = new AssemblyBotComponentEnumerator(AssemblyLoadContext.Default);

            Assert.Throws<FileNotFoundException>(
                () => enumerator.GetComponents(componentName: "Fake.Assembly.Name").ToList());
        }

        [Fact]
        public void GetPlugins_Throws_InvalidAssemblyName()
        {
            var enumerator = new AssemblyBotComponentEnumerator(AssemblyLoadContext.Default);

            Assert.Throws<FileLoadException>(
                () => enumerator.GetComponents(componentName: "*//\\-_+!?#$").ToList());
        }

        [Fact]
        public void GetPlugins_Succeeds()
        {
            var enumerator = new AssemblyBotComponentEnumerator(AssemblyLoadContext.Default);

            IList<BotComponent> components = enumerator.GetComponents(Assembly.GetExecutingAssembly().FullName).ToList();

            Assert.Equal(4, components.Count);
            Assert.Contains(components, p => typeof(PublicBotComponent) == p.GetType());
            Assert.Contains(components, p => typeof(AdventureWorksAdapterComponent) == p.GetType());
            Assert.Contains(components, p => typeof(ContosoAdapterComponent) == p.GetType());
            Assert.Contains(components, p => typeof(PirateBotComponent) == p.GetType());
        }

        // The below type definitions are intended purely for test usage to ensure that the assembly
        // contains protected and private implementations of IBotPlugin to ensure that they are not
        // returned by the enumerator. To avoid compilation errors, these must be defined as nested
        // class definitions.

        protected class ProtectedBotComponent : BotComponent
        {
            public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
            {
            }
        }

        private class PrivateBotBotComponent : BotComponent
        {
            public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
            {
            }
        }
    }
}
