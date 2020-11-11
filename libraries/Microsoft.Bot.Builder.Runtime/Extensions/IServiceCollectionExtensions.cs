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
    public static class IServiceCollectionExtensions
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
