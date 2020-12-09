// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Runtime.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Runtime.Extensions
{
    /// <summary>
    /// Defines extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds bot runtime-related services to the application's service collection.
        /// </summary>
        /// <param name="services">The application's collection of registered services.</param>
        /// <param name="configuration">The application configuration.</param>
        public static void AddBotRuntime(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // Component registrations must be added before the resource explorer is instantiated to ensure
            // that all types are correctly registered. Any types that are registered after the resource explorer
            // is instantiated will not be picked up otherwise.

            ComponentRegistrations.Add();

            string applicationRoot = configuration.GetSection(ConfigurationConstants.ApplicationRootKey).Value;

            services.AddBotRuntime(
                configuration,
                resourceExplorerImplementationFactory: (serviceProvider) =>
                    new ResourceExplorer()
                        .AddFolder(applicationRoot)
                        .RegisterType<OnQnAMatch>(OnQnAMatch.Kind));
        }

        /// <summary>
        /// Adds bot runtime-related services to the application's service collection.
        /// </summary>
        /// <remarks>
        /// For applications being developed utilizing the runtime, we expect that the configured
        /// <see cref="ResourceExplorer"/> will utilize declarative assets from the local development environment's
        /// file directories.
        ///
        /// However, as this would cause additional overhead for testing purposes, we expose this
        /// function solely to test assemblies to enable providing an instance of <see cref="ResourceExplorer"/>
        /// that loads resources using a custom in-memory provider.
        /// </remarks>
        /// <param name="services">The application's collection of registered services.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="resourceExplorerImplementationFactory">
        /// Function used to build an instance of <see cref="ResourceExplorer"/> from registered services.
        /// </param>
        internal static void AddBotRuntime(
            this IServiceCollection services,
            IConfiguration configuration,
            Func<IServiceProvider, ResourceExplorer> resourceExplorerImplementationFactory)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (resourceExplorerImplementationFactory == null)
            {
                throw new ArgumentNullException(nameof(resourceExplorerImplementationFactory));
            }

            services.AddSingleton<ResourceExplorer>(resourceExplorerImplementationFactory);

            using (IServiceScope serviceScope = services.BuildServiceProvider().CreateScope())
            {
                ResourceExplorer resourceExplorer =
                    serviceScope.ServiceProvider.GetRequiredService<ResourceExplorer>();

                Resource runtimeConfigurationResource =
                    resourceExplorer.GetResource(id: "runtime.json");
                var runtimeConfigurationProvider =
                    resourceExplorer.LoadType<RuntimeConfigurationProvider>(runtimeConfigurationResource);

                runtimeConfigurationProvider.ConfigureServices(services, configuration);
            }
        }
    }
}
