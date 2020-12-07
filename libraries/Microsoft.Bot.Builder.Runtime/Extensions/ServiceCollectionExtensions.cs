// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Runtime.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

#if SIGNASSEMBLY
[assembly: InternalsVisibleTo("Microsoft.Bot.Builder.Runtime.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
#else
[assembly: InternalsVisibleTo("Microsoft.Bot.Builder.Runtime.Tests")]
#endif

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
