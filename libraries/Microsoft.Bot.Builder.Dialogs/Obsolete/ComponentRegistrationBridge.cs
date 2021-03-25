// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder.Dialogs.Obsolete
{
    /// <summary>
    /// Bridge class to allow <see cref="BotComponent"/> to be backward compatible with <see cref="ComponentRegistration"/>.
    /// </summary>
    /// <typeparam name="TComponent">The type of <see cref="BotComponent"/> to bridge into legacy <see cref="ComponentRegistration"/>.</typeparam>
    [Obsolete("This class only exists for backward compatibility of legacy `ComponentRegistrations`. Use `BotComponent` for new components.")]
    public class ComponentRegistrationBridge<TComponent> : ComponentRegistration, IComponentMemoryScopes, IComponentPathResolvers
        where TComponent : BotComponent, new()
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegistrationBridge{TComponent}"/> class.
        /// </summary>
        /// <param name="botComponent"><see cref="BotComponent"/> to be exposed as a legacy <see cref="ComponentRegistration"/>.</param>
        /// <param name="configuration">Optional <see cref="IConfiguration"/> for the target <see cref="BotComponent"/>.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> for the target <see cref="BotComponent"/>.</param>
        public ComponentRegistrationBridge(TComponent botComponent, IConfiguration configuration = null, ILogger logger = null)
        {
            BotComponent = botComponent ?? throw new ArgumentNullException(nameof(botComponent));
            _configuration = configuration ?? new ConfigurationBuilder().Build();
            _logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegistrationBridge{TComponent}"/> class.
        /// </summary>
        public ComponentRegistrationBridge()
            : this(new TComponent())
        { 
        }

        /// <summary>
        /// Gets <see cref="BotComponent"/> to be bridged to the legacy <see cref="ComponentRegistration"/>.
        /// </summary>
        /// <value>
        /// <see cref="BotComponent"/> to be bridged to the legacy <see cref="ComponentRegistration"/>.
        /// </value>
        protected TComponent BotComponent { get; private set; }

        /// <inheritdoc/>
        public IEnumerable<MemoryScope> GetMemoryScopes()
        {
            return GetFromComponent<MemoryScope>();
        }

        /// <inheritdoc/>
        public IEnumerable<IPathResolver> GetPathResolvers()
        {
            return GetFromComponent<IPathResolver>();
        }

        /// <summary>
        /// Calls startup on the <see cref="BotComponent"/> to retrieve a collection of registrations for type <typeparamref name="TRegistration"/>.
        /// </summary>
        /// <typeparam name="TRegistration">The type of registration to get from the <see cref="BotComponent"/>.</typeparam>
        /// <param name="services">Optional initial <see cref="IServiceCollection"/> to pass to the <see cref="BotComponent"/>. If not provided, an empty <see cref="ServiceCollection"/> is used.</param>
        /// <returns>The collection of <typeparamref name="TRegistration"/> registrations applied by the <see cref="BotComponent"/>.</returns>
        protected IEnumerable<TRegistration> GetFromComponent<TRegistration>(IServiceCollection services = null)
        {
            services = services ?? new ServiceCollection();
            BotComponent.ConfigureServices(services, _configuration, _logger);
            return services.BuildServiceProvider().GetServices<TRegistration>();
        }
    }
}
