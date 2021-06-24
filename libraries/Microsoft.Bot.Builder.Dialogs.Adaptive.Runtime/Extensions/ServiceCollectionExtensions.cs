// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Component;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions
{
    /// <summary>
    /// Defines extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds bot runtime-related services to the application's service collection.
        /// </summary>
        /// <remark>
        /// The following dependencies are added with TrySingleton so advanced scenarios can be override them to customize the runtimne behavior:
        /// BotFrameworkAuthentication,
        /// IBot,
        /// ResourceExplorer,
        /// LanguagePolicy,
        /// ChannelServiceHandlerBase,
        /// IStorage,
        /// UserState,
        /// ConversationState,
        /// SkillConversationIdFactoryBase
        /// and IBotTelemetryClient.
        /// 
        /// Aspects of the behavior of a number of these dependencies, including those that can be overriden, can be controlled through configuration.   
        /// 
        /// The default ResourceExlorer uses the file system. The folder used being read from configuration.
        /// 
        /// The default LanguagePolicy is "us-en" this can be changed through configuration.
        /// 
        /// If not overriden, the exact type of storage added depends on configuration. With no configuration the default is memory storage.
        /// It should be noted that MemoryStorage is designed primarily for testing with a single host running the bot and no database.
        /// 
        /// The default Skills implementation can be constrained in terms of allowed callers through configuration.
        /// Refer to the product documentation for further details.
        /// 
        /// The default telemetry implementation used AppInsights and aspects of what is included in the telemetry can be controller through configuration.
        /// Refer to the product documentation for further details.
        /// 
        /// A number of the features of the runtime are implemented through middleware. Various feature flags in configuration determine whether these
        /// middleware are added at runtime, the settings include: UseInspection, RemoveRecipientMentions, ShowTyping and SetSpeak.
        /// 
        /// </remark>
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

            // Ensure the IConfiguration is available. (Azure Functions don't do this.)
            services.TryAddSingleton(configuration);

            // All things auth
            services.TryAddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // IBot
            services.TryAddSingleton<IBot, ConfigurationAdaptiveDialogBot>();

            // Resource explorer
            services.TryAddSingleton<ResourceExplorer, ConfigurationResourceExplorer>();

            // Language policy
            services.TryAddSingleton<LanguagePolicy, ConfigurationLanguagePolicy>();

            // CoreBotAdapter registration
            services.AddSingleton<CoreBotAdapter>();
            services.AddSingleton<IBotFrameworkHttpAdapter>(sp => sp.GetRequiredService<CoreBotAdapter>());

            // Needed for SkillsHttpClient which depends on BotAdapter
            services.AddSingleton<BotAdapter>(sp => sp.GetRequiredService<CoreBotAdapter>());

            // Runtime set up
            services.AddBotRuntimeSkills(configuration);
            services.AddBotRuntimeStorage();
            services.AddBotRuntimeTelemetry(configuration);
            services.AddBotRuntimeTranscriptLogging(configuration);
            services.AddBotRuntimeFeatures(configuration);
            services.AddBotRuntimeComponents(configuration);
        }

        internal static void AddBotRuntimeSkills(this IServiceCollection services, IConfiguration configuration)
        {
            var skillSettings = configuration.GetSection(SkillSettings.SkillSettingsKey).Get<SkillSettings>();

            services.AddSingleton(sp => new AuthenticationConfiguration { ClaimsValidator = new AllowedCallersClaimsValidator(skillSettings?.AllowedCallers) });
            services.TryAddSingleton<ChannelServiceHandlerBase, CloudSkillHandler>();
        }

        internal static void AddBotRuntimeStorage(this IServiceCollection services)
        {
            services.TryAddSingleton(ServiceFactory.Storage);
            services.TryAddSingleton<UserState>();
            services.TryAddSingleton<ConversationState>();
            services.TryAddSingleton<SkillConversationIdFactoryBase, SkillConversationIdFactory>();
        }

        internal static void AddBotRuntimeComponents(this IServiceCollection services, IConfiguration configuration)
        {
            var componentDefinitions = configuration
                .GetSection($"{ConfigurationConstants.RuntimeSettingsKey}:components")
                .Get<List<BotComponentDefinition>>() ?? Enumerable.Empty<BotComponentDefinition>();

            var componentEnumerator = new AssemblyBotComponentEnumerator(AssemblyLoadContext.Default);

            var exceptions = new List<Exception>();

            // Iterate through configured components and load each one
            foreach (BotComponentDefinition component in componentDefinitions)
            {
                try
                {
                    component.Load(componentEnumerator, services, configuration);
                }
#pragma warning disable CA1031 // Do not catch general exception types. We want to capture all exceptions from components and throw them all at once.
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException("Encountered exceptions while loading bot components.", exceptions);
            }

            // Load internal built-in components
            BuiltInBotComponents.LoadAll(services, configuration);
        }

        internal static void AddBotRuntimeTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            var telemetrySettings = configuration.GetSection(TelemetrySettings.TelemetrySettingsKey).Get<TelemetrySettings>();

            if (string.IsNullOrEmpty(telemetrySettings?.Options?.ConnectionString) && string.IsNullOrEmpty(telemetrySettings?.Options?.InstrumentationKey))
            {
                services.TryAddSingleton<IBotTelemetryClient, NullBotTelemetryClient>();
            }
            else
            {
                services.AddApplicationInsightsTelemetry(telemetrySettings.Options);
                services.TryAddSingleton<IBotTelemetryClient, BotTelemetryClient>();

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

        internal static void AddBotRuntimeTranscriptLogging(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var featureSettings = configuration.GetSection(FeatureSettings.FeaturesSettingsKey).Get<FeatureSettings>() ?? new FeatureSettings();

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

        internal static void AddBotRuntimeFeatures(this IServiceCollection services, IConfiguration configuration)
        {
            var featureSettings = configuration.GetSection(FeatureSettings.FeaturesSettingsKey).Get<FeatureSettings>() ?? new FeatureSettings();

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
                    featureSettings.SetSpeak.FallbackToTextForSpeechIfEmpty));
            }
        }
    }
}
