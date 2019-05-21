// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Tests
{
    /// <summary>
    /// A singleton class to access test configuration.
    /// </summary>
    public sealed class TestConfiguration
    {
        private static readonly Lazy<TestConfiguration> _configurationLazy = new Lazy<TestConfiguration>(() =>
        {
            LoadLaunchSettingsIntoEnvVariables("Properties//launchSettings.json");

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            return new TestConfiguration(config.Build());
        });

        private TestConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static TestConfiguration Instance => _configurationLazy.Value;

        public IConfiguration Configuration { get; }

        /// <summary>
        /// XUnit doesn't seem to load environment variables defined in launchSettings.json
        /// so this helper code loads it manually if the file is present.
        /// This is useful to be able to have your own key files in your local machine without
        /// having to put them in git.
        /// If you use launch settings, make sure you set copy always.
        /// </summary>
        /// <param name="launchSettingsFile">The relative path to the launch settings file (i.e.: "Properties\\launchSettings.json")</param>
        private static void LoadLaunchSettingsIntoEnvVariables(string launchSettingsFile)
        {
            if (!File.Exists(launchSettingsFile))
            {
                return;
            }

            using (var file = File.OpenText(launchSettingsFile))
            {
                var reader = new JsonTextReader(file);
                var fileData = JObject.Load(reader);

                var variables = fileData
                    .GetValue("profiles")
                    .SelectMany(profiles => profiles.Children())
                    .SelectMany(profile => profile.Children<JProperty>())
                    .Where(prop => prop.Name == "environmentVariables")
                    .SelectMany(prop => prop.Value.Children<JProperty>())
                    .ToList();

                foreach (var variable in variables)
                {
                    Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                }
            }
        }
    }
}
