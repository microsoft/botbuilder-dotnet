// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Configuration;
using System.Linq;
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
using Microsoft.Bot.Builder.Runtime.Plugins;
using Microsoft.Bot.Builder.Runtime.Settings;
using Microsoft.Bot.Builder.Runtime.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

            // System configuration
            string applicationRoot = configuration.GetSection(ConfigurationConstants.ApplicationRootKey).Value;
            string defaultLocale = configuration.GetSection(ConfigurationConstants.DefaultLocale).Value;
            string rootDialog = configuration.GetSection(ConfigurationConstants.RootDialogKey).Value;

            // Runtime configuration
            var runtimeSettings = configuration.GetSection(ConfigurationConstants.RuntimeSettingsKey).Get<RuntimeSettings>();

            services.AddOptions()
                .Configure<CoreBotOptions>(o =>
                {
                    o.DefaultLocale = defaultLocale;
                    o.RootDialog = rootDialog;
                });

            services.AddSingleton(configuration);

            // ResourceExplorer
            services.TryAddSingleton<ResourceExplorer>(serviceProvider =>
                new ResourceExplorer()
                    .AddFolder(applicationRoot)
                    .RegisterType<OnQnAMatch>(OnQnAMatch.Kind));

            // Bot
            services.AddSingleton<IBot, CoreBot>();
            
            // Runtime
            services.AddBotRuntimeSkills(runtimeSettings.Skills);
            services.AddBotRuntimeStorage(configuration, runtimeSettings);
            services.AddBotRuntimeTelemetry(runtimeSettings.Telemetry);
            services.AddBotRuntimeTranscriptLogging(configuration, runtimeSettings.Features);
            services.AddBotRuntimeFeatures(runtimeSettings.Features);
            services.AddBotRuntimePlugins(configuration, runtimeSettings);
            services.AddCoreBotAdapter();
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
            if (runtimeSettings?.Storage == typeof(CosmosDbPartitionedStorage).Name)
            {
                var cosmosDbOptions = configuration?.GetSection(typeof(CosmosDbPartitionedStorage).Name).Get<CosmosDbPartitionedStorageOptions>();
                services.AddSingleton<IStorage>(sp => new CosmosDbPartitionedStorage(cosmosDbOptions));
            }

            // Blob
            else if (runtimeSettings?.Storage == typeof(BlobsStorage).Name)
            {
                var blobOptions = configuration?.GetSection(typeof(BlobsStorage).Name).Get<BlobsStorageSettings>();
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
                var pluginEnumenator = serviceScope.ServiceProvider.GetService<IBotPluginEnumerator>() ?? new AssemblyBotPluginEnumerator(AssemblyLoadContext.Default);

                // Iterate through configured plugins and load each one
                foreach (BotPluginDefinition plugin in runtimeSettings.Plugins ?? Enumerable.Empty<BotPluginDefinition>())
                {
                    plugin.Load(pluginEnumenator, services, configuration);
                }
            }
        }

        internal static void AddCoreBotAdapter(this IServiceCollection services)
        {
            const string defaultRoute = "messages";

            // CoreAdapter dependencies registration
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton<IChannelProvider, ConfigurationChannelProvider>();

            // CoreAdapter registration
            services.AddSingleton<IBotFrameworkHttpAdapter, CoreBotAdapter>();
            services.AddSingleton<BotAdapter>(sp => sp.GetService<CoreBotAdapter>());
            
            // Adapter settings so the default adapter is homogeneous with the configured adapters at the controller / registration level
            services.AddSingleton(new AdapterSettings() { Route = defaultRoute, Enabled = true, Name = typeof(CoreBotAdapter).FullName });
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

            const string blobTranscriptSection = "blobTranscript";

            // Trace Trascript
            if (featureSettings.TraceTranscript)
            {
                services.AddSingleton<IMiddleware>(sp => new TranscriptLoggerMiddleware(new TraceTranscriptLogger()));
            }

            // Blob transcript
            if (featureSettings.BlobTranscript)
            {
                var blobOptions = configuration.GetSection(blobTranscriptSection).Get<BlobsStorageSettings>();

                if (blobOptions != null)
                {
                    var transcriptStore = new BlobsTranscriptStore(blobOptions.ConnectionString, blobOptions.ContainerName);
                    services.AddSingleton<IMiddleware>(sp => new TranscriptLoggerMiddleware(transcriptStore));
                }
                else
                {
                    throw new ConfigurationException("Blob transcript is enabled but no blob transcript store configuration was found.");
                }
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
