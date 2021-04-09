// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Definition of a <see cref="BotComponent"/> that allows registration of services, custom actions, memory scopes and adapters.
    /// </summary>
    /// To make your components available to the system you derive from BotComponent and register services to add functionality.
    /// These components then are consumed in appropriate places by the systems that need them. When using Composer, Startup gets called
    /// automatically on the components by the bot runtime, as long as the components are registered in the configuration.
    public abstract class BotComponent
    {
        /// <summary>
        /// Entry point for bot components to register types in resource explorer, consume configuration and register services in the 
        /// services collection.
        /// </summary>
        /// <param name="services">Services collection to register dependency injection.</param>
        /// <param name="configuration">Configuration for the bot component.</param>
        public abstract void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration);
    }
}
