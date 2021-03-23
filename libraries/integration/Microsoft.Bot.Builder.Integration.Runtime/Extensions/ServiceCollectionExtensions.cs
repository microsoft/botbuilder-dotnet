// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Loader;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Integration.Runtime.Plugins;
using Microsoft.Bot.Builder.Integration.Runtime.Settings;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Bot.Builder.Integration.Runtime.Extensions
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
        public static void AddAdaptiveRuntime(this IServiceCollection services)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));

            services.AddSingleton<ResourceExplorer, ConfigurationResourceExplorer>();
            services.AddSingleton<IBot, ConfigurationAdaptiveDialogBot>();

            // TODO: add CloudAdapter derived class - including telemetry and middleware and transcripts
            // TODO: add Azure storage defaults
            // TODO: add Skills
        }

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

            // Configuration
            string applicationRoot = configuration.GetSection(ConfigurationConstants.ApplicationRootKey).Value;
            string defaultLocale = configuration.GetSection(ConfigurationConstants.DefaultLocaleKey).Value;
            string rootDialog = configuration.GetSection(ConfigurationConstants.RootDialogKey).Value;
            
            // Runtime settings. If no config is provided, we create basic runtime config with defaults.
            var runtimeSettings = configuration.GetSection(ConfigurationConstants.RuntimeSettingsKey).Get<RuntimeSettings>() ?? new RuntimeSettings();

            // Bot
            services.AddSingleton<IBot, CoreBot>();
            services.AddOptions()
                .Configure<CoreBotOptions>(o =>
                {
                    o.DefaultLocale = defaultLocale;
                    o.RootDialog = rootDialog;
                });

            // ResourceExplorer. TryAddSingleton will only add if there is no other registration for resource explorer.
            // Tests use this to inject custom resource explorers but could also be used for advanced runtime customization scenarios.
            services.TryAddSingleton<ResourceExplorer>(serviceProvider =>
                new ResourceExplorer()
                    .AddFolder(applicationRoot)
                    .RegisterType<OnQnAMatch>(OnQnAMatch.Kind));
            
            // Runtime set up
            services.AddBotRuntimeSkills(runtimeSettings.Skills);
            services.AddBotRuntimeStorage(configuration, runtimeSettings);
            services.AddBotRuntimeTelemetry(runtimeSettings.Telemetry);
            services.AddBotRuntimeTranscriptLogging(configuration, runtimeSettings.Features);
            services.AddBotRuntimeFeatures(runtimeSettings.Features);
            services.AddBotRuntimePlugins(configuration, runtimeSettings);
            services.AddBotRuntimeAdapters(runtimeSettings);
        }

        internal static void AddBotRuntimeSkills(this IServiceCollection services, SkillSettings skillSettings)
        {
            services.AddSingleton(sp => new AuthenticationConfiguration { ClaimsValidator = new AllowedCallersClaimsValidator(skillSettings?.AllowedCallers) });
            services.AddSingleton<SkillConversationIdFactoryBase, SkillConversationIdFactory>();
            services.AddHttpClient<BotFrameworkClient, SkillHttpClient>();
            services.AddSingleton<ChannelServiceHandler, SkillHandler>();
        }

        internal static void AddBotRuntimeStorage(this IServiceCollection services, IConfiguration configuration, RuntimeSettings runtimeSettings)
        {
            services.AddSingleton<UserState>();
            services.AddSingleton<ConversationState>();

            // Cosmosdb
            if (runtimeSettings?.Storage == nameof(CosmosDbPartitionedStorage))
            {
                var cosmosDbOptions = configuration?.GetSection(nameof(CosmosDbPartitionedStorage)).Get<CosmosDbPartitionedStorageOptions>();
                services.AddSingleton<IStorage>(sp => new CosmosDbPartitionedStorage(cosmosDbOptions));
            }

            // Blob
            else if (runtimeSettings?.Storage == nameof(BlobsStorage))
            {
                var blobOptions = configuration?.GetSection(nameof(BlobsStorage)).Get<BlobsStorageSettings>();
                services.AddSingleton<IStorage>(sp => new BlobsStorage(blobOptions?.ConnectionString, blobOptions?.ContainerName));
            }

            // Default
            else
            {
                // If no storage is configured, default to memory storage
                services.AddSingleton<IStorage, MemoryStorage>();
            }
        }

        internal static void AddBotRuntimePlugins(this IServiceCollection services, IConfiguration configuration, RuntimeSettings runtimeSettings)
        {
            using (IServiceScope serviceScope = services.BuildServiceProvider().CreateScope())
            {
                var pluginEnumerator = serviceScope.ServiceProvider.GetService<IBotPluginEnumerator>() ?? new AssemblyBotPluginEnumerator(AssemblyLoadContext.Default);

                // Iterate through configured plugins and load each one
                foreach (BotPluginDefinition plugin in runtimeSettings.Plugins)
                {
                    plugin.Load(pluginEnumerator, services, configuration);
                }
            }
        }

        internal static void AddBotRuntimeAdapters(this IServiceCollection services, RuntimeSettings runtimeSettings)
        {
            const string defaultRoute = "messages";

            // CoreAdapter dependencies registration
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton<IChannelProvider, ConfigurationChannelProvider>();

            // CoreAdapter registration
            services.AddSingleton<CoreBotAdapter>();
            services.AddSingleton<IBotFrameworkHttpAdapter, CoreBotAdapter>();

            // Needed for SkillsHttpClient which depends on BotAdapter
            services.AddSingleton<BotAdapter, CoreBotAdapter>(); 
            
            // Adapter settings so the default adapter is homogeneous with the configured adapters at the controller / registration level
            services.AddSingleton(new AdapterSettings() { Route = defaultRoute, Enabled = true, Name = typeof(CoreBotAdapter).FullName });

            // Adapter settings for configurable adapters. Runtime controllers pick up this config to get info on adapters and routes.
            foreach (var adapterSetting in runtimeSettings.Adapters)
            {
                services.AddSingleton<AdapterSettings>(adapterSetting);
            }
        }

        internal static void AddBotRuntimeTelemetry(this IServiceCollection services, TelemetrySettings telemetrySettings = null)
        {
            if (string.IsNullOrEmpty(telemetrySettings?.InstrumentationKey))
            {
                services.AddSingleton<IBotTelemetryClient, NullBotTelemetryClient>();
            }
            else
            {
                services.AddApplicationInsightsTelemetry(telemetrySettings.InstrumentationKey);
                services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();

                services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
                services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();
                services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();

                services.AddSingleton<IMiddleware>(sp =>
                {
                    var botTelemetryClient = sp.GetService<IBotTelemetryClient>();
                    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();

                    return new TelemetryInitializerMiddleware(
                       httpContextAccessor: httpContextAccessor,
                       telemetryLoggerMiddleware: new TelemetryLoggerMiddleware(
                           telemetryClient: botTelemetryClient,
                           logPersonalInformation: telemetrySettings.LogPersonalInformation),
                       logActivityTelemetry: telemetrySettings.LogActivities);
                });
            }
        }

        internal static void AddBotRuntimeTranscriptLogging(this IServiceCollection services, IConfiguration configuration, FeatureSettings featureSettings)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // Trace Transcript
            if (featureSettings.TraceTranscript)
            {
                services.AddSingleton<IMiddleware>(sp => new TranscriptLoggerMiddleware(new TraceTranscriptLogger()));
            }

            // Blob transcript
            var blobOptions = featureSettings.BlobTranscript;
            if (blobOptions != null)
            {
                var transcriptStore = new BlobsTranscriptStore(blobOptions.ConnectionString, blobOptions.ContainerName);
                services.AddSingleton<IMiddleware>(sp => new TranscriptLoggerMiddleware(transcriptStore));
            }
        }

        internal static void AddBotRuntimeFeatures(this IServiceCollection services, FeatureSettings featureSettings)
        {
            if (featureSettings.UseInspection)
            {
                services.AddSingleton<InspectionState>();
                services.AddSingleton<IMiddleware, InspectionMiddleware>();
            }

            if (featureSettings.RemoveRecipientMentions)
            {
                services.AddSingleton<NormalizeMentionsMiddleware>();
            }

            if (featureSettings.ShowTyping)
            {
                services.AddSingleton<ShowTypingMiddleware>();
            }
        }
    }
}
