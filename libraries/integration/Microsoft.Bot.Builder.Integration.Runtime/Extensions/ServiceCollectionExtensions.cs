// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Loader;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration.Runtime.Component;
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
          
            // Runtime settings. If no config is provided, we create basic runtime config with defaults.
            var runtimeSettings = configuration.GetSection(ConfigurationConstants.RuntimeSettingsKey).Get<RuntimeSettings>() ?? new RuntimeSettings();

            // Ensure the IConfiguration is available. (Azure Functions don't do this.)
            services.TryAddSingleton(sp => configuration);

            // All things auth
            services.TryAddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // IBot
            services.AddSingleton<IBot, ConfigurationAdaptiveDialogBot>();

            // Resource explorer
            services.TryAddSingleton<ResourceExplorer, ConfigurationResourceExplorer>();

            // Runtime set up
            services.AddBotRuntimeSkills(runtimeSettings.Skills);
            services.AddBotRuntimeStorage();
            services.AddBotRuntimeAdapters(runtimeSettings);
            services.AddBotRuntimeTelemetry(runtimeSettings.Telemetry);
            services.AddBotRuntimeTranscriptLogging(configuration, runtimeSettings.Features);
            services.AddBotRuntimeFeatures(runtimeSettings.Features);
            services.AddBotRuntimeComponents(configuration, runtimeSettings);
        }

        internal static void AddBotRuntimeSkills(this IServiceCollection services, SkillSettings skillSettings)
        {
            services.AddSingleton(sp => new AuthenticationConfiguration { ClaimsValidator = new AllowedCallersClaimsValidator(skillSettings?.AllowedCallers) });
            services.AddSingleton<ChannelServiceHandlerBase, CloudSkillHandler>();
        }

        internal static void AddBotRuntimeStorage(this IServiceCollection services)
        {
            services.TryAddSingleton(ServiceFactory.Storage);
            services.TryAddSingleton<UserState>();
            services.TryAddSingleton<ConversationState>();
            services.TryAddSingleton<SkillConversationIdFactoryBase, SkillConversationIdFactory>();
        }

        internal static void AddBotRuntimeComponents(this IServiceCollection services, IConfiguration configuration, RuntimeSettings runtimeSettings)
        {
            using (IServiceScope serviceScope = services.BuildServiceProvider().CreateScope())
            {
                var componentEnumenator = serviceScope.ServiceProvider.GetService<IBotComponentEnumerator>() ?? new AssemblyBotComponentEnumerator(AssemblyLoadContext.Default);

                // Iterate through configured components and load each one
                foreach (BotComponentDefinition component in runtimeSettings.Components)
                {
                    component.Load(componentEnumenator, services, configuration);
                }
            }

            foreach (BotComponent component in BuiltInBotComponents.GetComponents())
            {
                var componentServices = new ServiceCollection();

                component.ConfigureServices(componentServices, configuration, null /*for now*/);

                foreach (var serviceDescriptor in componentServices)
                {
                    services.Add(serviceDescriptor);
                }
            }
        }

        internal static void AddBotRuntimeAdapters(this IServiceCollection services, RuntimeSettings runtimeSettings)
        {
            const string defaultRoute = "messages";

            // CoreBotAdapter registration
            services.AddSingleton<CoreBotAdapter>();
            services.AddSingleton<IBotFrameworkHttpAdapter>(sp => sp.GetRequiredService<CoreBotAdapter>());

            // Needed for SkillsHttpClient which depends on BotAdapter
            services.AddSingleton<BotAdapter>(sp => sp.GetRequiredService<CoreBotAdapter>()); 
            
            // Adapter settings so the default adapter is homogeneous with the configured adapters at the controller / registration level
            services.AddSingleton(new AdapterSettings() { Route = defaultRoute, Enabled = true, Name = typeof(CoreBotAdapter).FullName });

            // Adapter settings for configurable adapters. Runtime controllers pick up this config to get info on adapters and routes.
            foreach (var adapterSetting in runtimeSettings.Adapters)
            {
                services.AddSingleton(adapterSetting);
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
                services.AddSingleton<IMiddleware, NormalizeMentionsMiddleware>();
            }

            if (featureSettings.ShowTyping)
            {
                services.AddSingleton<IMiddleware, ShowTypingMiddleware>();
            }

            if (featureSettings.SetSpeak != null)
            {
                services.AddSingleton<IMiddleware>(sp => new SetSpeakMiddleware(
                    featureSettings.SetSpeak.VoiceFontName, 
                    featureSettings.SetSpeak.Lang, 
                    featureSettings.SetSpeak.FallbackToTextForSpeechIfEmpty));
            }
        }
    }
}
