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
    public static class ServiceCollectionExtensions
    {
        public static void AddBotCore(this IServiceCollection services, IConfiguration configuration)
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

            services.AddBotCore(
                configuration,
                resourceExplorerImplementationFactory: (serviceProvider) =>
                    new ResourceExplorer()
                        .AddFolder(applicationRoot)
                        .RegisterType<OnQnAMatch>(OnQnAMatch.Kind));
        }

        internal static void AddBotCore(
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
