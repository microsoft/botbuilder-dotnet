// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.BotFramework.Composer.Core;
using Microsoft.BotFramework.Composer.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

[assembly: FunctionsStartup(typeof(Microsoft.BotFramework.Composer.Functions.Startup))]

namespace Microsoft.BotFramework.Composer.Functions
{
    public class Startup : FunctionsStartup
    {
        private IConfigurationRoot BuildConfiguration(string rootDirectory)
        {
            var config = new ConfigurationBuilder();

            // Config precedence 1: root app.settings
            config
                .SetBasePath(rootDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .UseLuisConfigAdapter()
                .UseLuisSettings();

            // Config precedence 2: ComposerDialogs/settings settings which are injected by the composer publish
            // Hard code the settings path to 'ComposerDialogs' for deployment
            var configFile = Path.GetFullPath(Path.Combine(rootDirectory, @"ComposerDialogs/settings/appsettings.json"));
            config.AddJsonFile(configFile, optional: true, reloadOnChange: true);

            // Config Precedence 3: Deployment specific config
            config.AddJsonFile("appsettings.deployment.json", optional: true, reloadOnChange: true);

            if (!Debugger.IsAttached)
            {
                config.AddUserSecrets<Startup>();
            }

            config.AddEnvironmentVariables();

            return config.Build();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var rootDirectory = Directory.GetParent(binDirectory).FullName;

            var rootConfiguration = BuildConfiguration(rootDirectory);

            var settings = new BotSettings();
            rootConfiguration.Bind(settings);

            var services = builder.Services;

            services.AddSingleton<IConfiguration>(rootConfiguration);

            services.AddLogging();

            // Create the credential provider to be used with the Bot Framework Adapter.
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton<BotAdapter>(sp => (BotFrameworkHttpAdapter)sp.GetService<IBotFrameworkHttpAdapter>());

            // Register AuthConfiguration to enable custom claim validation.
            services.AddSingleton<AuthenticationConfiguration>();

            // Adaptive component registration
            ComponentRegistration.Add(new DialogsComponentRegistration());
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new QnAMakerComponentRegistration());
            ComponentRegistration.Add(new LuisComponentRegistration());

            // Register the skills client and skills request handler.
            services.AddSingleton<SkillConversationIdFactoryBase, SkillConversationIdFactory>();
            services.AddHttpClient<BotFrameworkClient, SkillHttpClient>();
            services.AddSingleton<ChannelServiceHandler, SkillHandler>();

            // Register telemetry client, initializers and middleware
            services.AddApplicationInsightsTelemetry(settings.ApplicationInsights.InstrumentationKey);
            services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();
            services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();
            services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();
            services.AddSingleton<TelemetryLoggerMiddleware>(sp =>
            {
                var telemetryClient = sp.GetService<IBotTelemetryClient>();
                return new TelemetryLoggerMiddleware(telemetryClient, logPersonalInformation: settings.Telemetry.LogPersonalInformation);
            });
            services.AddSingleton<TelemetryInitializerMiddleware>(sp =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var telemetryLoggerMiddleware = sp.GetService<TelemetryLoggerMiddleware>();
                return new TelemetryInitializerMiddleware(httpContextAccessor, telemetryLoggerMiddleware, settings.Telemetry.LogActivities);
            });

            // Storage
            IStorage storage;
            if (ConfigSectionValid(settings.CosmosDb.AuthKey))
            {
                storage = new CosmosDbPartitionedStorage(settings.CosmosDb);
            }
            else
            {
                storage = new MemoryStorage();
            }

            services.AddSingleton(storage);
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);
            services.AddSingleton(userState);
            services.AddSingleton(conversationState);

            // Resource explorer to track declarative assets
            var resourceExplorer = new ResourceExplorer().AddFolder(Path.Combine(rootDirectory, settings.Bot ?? "."));
            services.AddSingleton(resourceExplorer);

            // Adapter
            services.AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkHttpAdapter>(s =>
            {
                // Retrieve required dependencies
                //IConfiguration configuration = s.GetService<IConfiguration>();
                IStorage storage = s.GetService<IStorage>();
                UserState userState = s.GetService<UserState>();
                ConversationState conversationState = s.GetService<ConversationState>();
                TelemetryInitializerMiddleware telemetryInitializerMiddleware = s.GetService<TelemetryInitializerMiddleware>();

                var adapter = new BotFrameworkHttpAdapter(new ConfigurationCredentialProvider(rootConfiguration));

                adapter
                  .UseStorage(storage)
                  .UseBotState(userState, conversationState)
                  .Use(telemetryInitializerMiddleware);

                // Configure Middlewares
                ConfigureTranscriptLoggerMiddleware(adapter, settings);
                ConfigureInspectionMiddleWare(adapter, settings, s);
                ConfigureShowTypingMiddleWare(adapter, settings);

                adapter.OnTurnError = async (turnContext, exception) =>
                {
                    await turnContext.SendActivityAsync(exception.Message).ConfigureAwait(false);
                    await conversationState.ClearStateAsync(turnContext).ConfigureAwait(false);
                    await conversationState.SaveChangesAsync(turnContext).ConfigureAwait(false);
                };

                return adapter;
            });

            var defaultLocale = rootConfiguration.GetValue<string>("defaultLocale") ?? "en-us";

            // Bot
            services.AddSingleton<IBot>(s =>
                new ComposerBot(
                    s.GetService<ConversationState>(),
                    s.GetService<UserState>(),
                    s.GetService<ResourceExplorer>(),
                    s.GetService<BotFrameworkClient>(),
                    s.GetService<SkillConversationIdFactoryBase>(),
                    s.GetService<IBotTelemetryClient>(),
                    GetRootDialog(Path.Combine(rootDirectory, settings.Bot)),
                    defaultLocale));
        }

        public void ConfigureTranscriptLoggerMiddleware(BotFrameworkHttpAdapter adapter, BotSettings settings)
        {
            if (ConfigSectionValid(settings.BlobStorage.ConnectionString) && ConfigSectionValid(settings.BlobStorage.Container))
            {
                adapter.Use(new TranscriptLoggerMiddleware(new AzureBlobTranscriptStore(settings.BlobStorage.ConnectionString, settings.BlobStorage.Container)));
            }
        }

        public void ConfigureShowTypingMiddleWare(BotFrameworkAdapter adapter, BotSettings settings)
        {
            if (settings.Feature.UseShowTypingMiddleware)
            {
                adapter.Use(new ShowTypingMiddleware());
            }
        }

        public void ConfigureInspectionMiddleWare(BotFrameworkAdapter adapter, BotSettings settings, IServiceProvider s)
        {
            if (settings.Feature.UseInspectionMiddleware)
            {
                adapter.Use(s.GetService<TelemetryInitializerMiddleware>());
            }
        }

        private string GetRootDialog(string folderPath)
        {
            var dir = new DirectoryInfo(folderPath);
            foreach (var f in dir.GetFiles())
            {
                if (f.Extension == ".dialog")
                {
                    return f.Name;
                }
            }

            throw new Exception($"Can't locate root dialog in {dir.FullName}");
        }

        private bool ConfigSectionValid(string val)
        {
            return !string.IsNullOrEmpty(val) && !val.StartsWith('<');
        }
    }
}
