// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions
{
    /// <summary>
    /// Extensions for setting up Runtime IConfiguration.
    /// </summary>
    public static class ConfigurationBuilderExtensions
    {
        private const string DialogFileExtension = ".dialog";

        /// <summary>
        /// Setup the provided <see cref="IConfigurationBuilder"/> with the required Runtime configuration.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="IConfigurationBuilder"/> to supply with additional in-memory configuration settings.
        /// </param>
        /// <param name="hostBuilderContext">A <see cref="HostBuilderContext"/> instance with information about the host.</param>
        /// <param name="applicationRoot">
        /// The application root directory. When running in local development mode from Composer, this is determined
        /// to be three directory levels above where the runtime application project is ejected, i.e. '../../..'.
        /// </param>
        /// <returns>
        /// Supplied <see cref="IConfigurationBuilder"/> instance with additional in-memory configuration provider.
        /// </returns>
        public static IConfigurationBuilder AddBotRuntimeConfiguration(this IConfigurationBuilder builder, HostBuilderContext hostBuilderContext, string applicationRoot)
        {
            return AddBotRuntimeConfiguration(builder, hostBuilderContext, applicationRoot, null);
        }

        /// <summary>
        /// Setup the provided <see cref="IConfigurationBuilder"/> with the required Runtime configuration.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="IConfigurationBuilder"/> to supply with additional in-memory configuration settings.
        /// </param>
        /// <param name="hostBuilderContext">A <see cref="HostBuilderContext"/> instance with information about the host.</param>
        /// <param name="applicationRoot">
        /// The application root directory. When running in local development mode from Composer, this is determined
        /// to be three directory levels above where the runtime application project is ejected, i.e. '../../..'.
        /// </param>
        /// <param name="settingsDirectory">
        /// The relative path to the directory containing the appsettings.json file to add as a configuration source.
        /// If null is specified, appsettings.json will be located within the application root directory.
        /// </param>
        /// <returns>
        /// Supplied <see cref="IConfigurationBuilder"/> instance with additional in-memory configuration provider.
        /// </returns>
        public static IConfigurationBuilder AddBotRuntimeConfiguration(
            this IConfigurationBuilder builder,
            HostBuilderContext hostBuilderContext,
            string applicationRoot,
            string settingsDirectory)
        {
            if (hostBuilderContext == null)
            {
                throw new ArgumentNullException(nameof(hostBuilderContext));
            }

            if (string.IsNullOrWhiteSpace(applicationRoot))
            {
                throw new ArgumentNullException(nameof(applicationRoot));
            }

            // Add in-memory properties for the bot runtime that depend on the environment and application root
            AddBotRuntimeProperties(builder, applicationRoot);

            // Load appsettings.
            var appSettingsConfigFilePath = Path.Combine(applicationRoot, settingsDirectory, "appsettings.json");
            var developerSettingsConfigFilePath = Path.Combine(applicationRoot, settingsDirectory, $"appsettings.{hostBuilderContext.HostingEnvironment.EnvironmentName}.json");
            builder.AddJsonFile(appSettingsConfigFilePath, optional: true, reloadOnChange: true)
                .AddJsonFile(developerSettingsConfigFilePath, optional: true, reloadOnChange: true);

            // Use Composer luis and qna settings extensions
            builder.AddComposerConfiguration();

            builder.AddEnvironmentVariables();

            return builder;
        }

        /// <summary>
        /// Provides a collection of in-memory configuration values for the bot runtime to
        /// the provided <see cref="IConfigurationBuilder"/>.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="IConfigurationBuilder"/> to supply with additional in-memory configuration settings.
        /// </param>
        /// <param name="applicationRoot">
        /// The application root directory. When running in local development mode from Composer, this is determined
        /// to be three directory levels above where the runtime application project is ejected, i.e. '../../..'.
        /// </param>
        private static void AddBotRuntimeProperties(IConfigurationBuilder builder, string applicationRoot)
        {
            var settings = new Dictionary<string, string>
            {
                { ConfigurationConstants.ApplicationRootKey, applicationRoot },
                { ConfigurationConstants.RootDialogKey, GetDefaultRootDialog(applicationRoot) },
            };

            builder.AddInMemoryCollection(settings);
        }

        /// <summary>
        /// Setup configuration to utilize the settings file generated by bf luis:build and qna:build. This is a luis
        /// and qnamaker settings extensions adapter aligning with Composer customized settings.
        /// </summary>
        /// <remarks>
        /// This will pick up LUIS_AUTHORING_REGION or --region settings as the setting to target.
        /// This will pick up --environment as the environment to target.  If environment is Development it will use
        /// the name of the logged in user.
        /// This will pick up --root as the root folder to run in.
        /// </remarks>
        /// <param name="builder">Configuration builder to modify.</param>
        /// <returns>Modified configuration builder.</returns>
        private static IConfigurationBuilder AddComposerConfiguration(this IConfigurationBuilder builder)
        {
            var configuration = builder.Build();
            var botRoot = configuration.GetValue<string>("bot") ?? ".";
            var luisRegion = configuration.GetValue<string>("LUIS_AUTHORING_REGION")
                ?? configuration.GetValue<string>("luis:authoringRegion")
                ?? configuration.GetValue<string>("luis:region") ?? "westus";
            var qnaRegion = configuration.GetValue<string>("qna:qnaRegion") ?? "westus";
            var environment = configuration.GetValue<string>("luis:environment") ?? Environment.UserName;
            var settings = new Dictionary<string, string>();
            var luisEndpoint = configuration.GetValue<string>("luis:endpoint");
            if (string.IsNullOrWhiteSpace(luisEndpoint))
            {
                luisEndpoint = $"https://{luisRegion}.api.cognitive.microsoft.com";
            }

            settings["luis:endpoint"] = luisEndpoint;
            settings["BotRoot"] = botRoot;
            builder.AddInMemoryCollection(settings);
            if (environment == "Development")
            {
                environment = Environment.UserName;
            }

            var luisSettingsPath = Path.GetFullPath(Path.Combine(botRoot, "generated", $"luis.settings.{environment.ToLowerInvariant()}.{luisRegion}.json"));
            var luisSettingsFile = new FileInfo(luisSettingsPath);
            if (luisSettingsFile.Exists)
            {
                builder.AddJsonFile(luisSettingsFile.FullName, optional: false, reloadOnChange: true);
            }

            var qnaSettingsPath = Path.GetFullPath(Path.Combine(botRoot, "generated", $"qnamaker.settings.{environment.ToLowerInvariant()}.{qnaRegion}.json"));
            var qnaSettingsFile = new FileInfo(qnaSettingsPath);
            if (qnaSettingsFile.Exists)
            {
                builder.AddJsonFile(qnaSettingsFile.FullName, optional: false, reloadOnChange: true);
            }

            var orchestratorSettingsPath = Path.GetFullPath(Path.Combine(botRoot, "generated", "orchestrator.settings.json"));
            var orchestratorSettingsFile = new FileInfo(orchestratorSettingsPath);
            if (orchestratorSettingsFile.Exists)
            {
                builder.AddJsonFile(orchestratorSettingsFile.FullName, optional: false, reloadOnChange: true);
            }

            return builder;
        }

        private static string GetDefaultRootDialog(string botRoot)
        {
            var directory = new DirectoryInfo(botRoot);
            foreach (var file in directory.GetFiles())
            {
                if (string.Equals(DialogFileExtension, file.Extension, StringComparison.OrdinalIgnoreCase))
                {
                    return file.Name;
                }
            }

            return null;
        }
    }
}
