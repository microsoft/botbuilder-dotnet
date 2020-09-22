// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Bot.Builder.ApplicationInsights;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Core.Tests
{
    [Trait("TestCategory", "ApplicationInsights")]
    public class ServiceResolutionTests
    {
        [Fact]
        public void AppSettings_NoBot_NoAppSettings()
        {
            ArrangeBotFile(null); // No bot file
            ArrangeAppSettings(null); // No appsettings file
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<StartupAppSettingsOnly>());

            // Telemetry Client should be active, just not configured.
            // This is not an error condition so samples can degrade.
            var telemetryClient = server.Host.Services.GetService(typeof(IBotTelemetryClient));
            Assert.NotNull(telemetryClient);
            Assert.Equal(typeof(NullBotTelemetryClient), telemetryClient.GetType());

            // App Insights Telemetry obviously can't work.
            Assert.Null(server.Host.Services.GetService(typeof(TelemetryClient)));
        }

        [Fact]
        public void AppSettings_NoBot_AppSettings_NoInstrumentation()
        {
            ArrangeBotFile(null); // No bot file
            ArrangeAppSettings("no_instrumentation_key"); // Appsettings file with no instrumentation key
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<StartupAppSettingsOnly>());

            // Telemetry Client should be active, just not configured.
            // This is not an error condition so samples can degrade.
            var telemetryClient = server.Host.Services.GetService(typeof(IBotTelemetryClient));
            Assert.NotNull(telemetryClient);
            Assert.Equal(typeof(NullBotTelemetryClient), telemetryClient.GetType());

            // App Insights Telemetry obviously can't work.
            Assert.Null(server.Host.Services.GetService(typeof(TelemetryClient)));
        }

        [Fact]
        public void AppSettings_NoBot_AppSettings()
        {
            ArrangeBotFile(null); // No bot file
            ArrangeAppSettings("default"); // Appsettings file with instrumentation key
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<StartupAppSettingsOnly>());

            // Telemetry Client should be active
            var telemetryClient = server.Host.Services.GetService(typeof(IBotTelemetryClient));
            Assert.NotNull(telemetryClient);
            Assert.NotEqual(typeof(NullBotTelemetryClient), telemetryClient.GetType());
            Assert.NotNull(server.Host.Services.GetService(typeof(TelemetryClient)));
        }

        [Fact]
        public void AppSettings_NoConfig_AppSettings()
        {
            ArrangeBotFile(null); // No bot file
            ArrangeAppSettings("default"); // Appsettings file with instrumentation key
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<StartupNoParameters>());

            // Telemetry Client should be active
            var telemetryClient = server.Host.Services.GetService(typeof(IBotTelemetryClient));
            Assert.NotNull(telemetryClient);
            Assert.NotEqual(typeof(NullBotTelemetryClient), telemetryClient.GetType());
            Assert.NotNull(server.Host.Services.GetService(typeof(TelemetryClient)));
        }

        [Fact]
        public void AppSettings_NoBot_AppSettings_InvalidKey()
        {
            ArrangeBotFile(null); // No bot file
            ArrangeAppSettings("invalid_instrumentation_key"); // Appsettings file with invalid instrumentation key
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<StartupAppSettingsOnly>());

            // Bot Telemetry Client should be active.
            // This is not an error condition so samples can degrade.
            var telemetryClient = server.Host.Services.GetService(typeof(IBotTelemetryClient));
            Assert.NotNull(telemetryClient);
            Assert.Equal(typeof(BotTelemetryClient), telemetryClient.GetType());

            // App Insights just rolls with it.  It's technically invalid, but App Insights doesn't (currently) error even when logging.
            Assert.NotNull(server.Host.Services.GetService(typeof(TelemetryClient)));
        }

        [Fact]
        public void Botfile_NoAppSettings()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings(null); // No appsettings file
            new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());

            // Telemetry Client should be active, just not configured.
            // This is not an error condition so samples can degrade.
            var telemetryClient = new TelemetryClient();
            Assert.True(string.IsNullOrWhiteSpace(telemetryClient.InstrumentationKey));
        }

        [Fact]
        public void Botfile_NoAppInsights()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings("no_app_insights"); // Bad app insights appsettings file

            new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());

            // Telemetry Client should be active, just not configured.
            // This is not an error condition so samples can degrade.
            var telemetryClient = new TelemetryClient();
            Assert.True(string.IsNullOrWhiteSpace(telemetryClient.InstrumentationKey));
        }

        [Fact]
        public void Botfile_NoAppInsightsKey()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings("no_instrumentation_key"); // Bad app insights appsettings file

            new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());

            // Telemetry Client should be active, just not configured.
            // This is not an error condition so samples can degrade.
            var telemetryClient = new TelemetryClient();
            Assert.True(string.IsNullOrWhiteSpace(telemetryClient.InstrumentationKey));
        }

        [Fact]
        public void BotFile_NoBotFile()
        {
            Assert.Throws<FileNotFoundException>(() =>
            {
                ArrangeBotFile(null); // No bot file
                ArrangeAppSettings(); // Default app settings
                new TestServer(new WebHostBuilder()
                    .UseStartup<Startup>());
            });
        }

        [Fact]
        public void BotFile_NoAppInsightsInBot()
        {
            // Should default to the Null TelemetryClient
            ArrangeBotFile("no_app_insights"); // Invalid bot file
            ArrangeAppSettings(); // Default app settings
            new TestServer(new WebHostBuilder()
                .UseStartup<StartupVerifyNullTelemetry>());
        }

        [Fact]
        public void ServiceResolution_GoodLoad()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings(); // Default app settings
            new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
        }

        [Fact]
        public void ServiceResolution_VerifyTelemetryClient()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings(); // Default app settings
            new TestServer(new WebHostBuilder()
                .UseApplicationInsights()
                .UseStartup<StartupVerifyTelemetryClient>());
        }

        [Fact]
        public void ServiceResolution_OverrideTelemetry()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings(); // Default app settings
            new TestServer(new WebHostBuilder()
                .UseStartup<StartupOverideTelemClient>());
        }

        [Fact]
        public void ServiceResolution_MultipleAppInsights()
        {
            ArrangeBotFile("multiple_app_insights"); // Default bot file
            ArrangeAppSettings(); // Default app settings
            new TestServer(new WebHostBuilder()
                .UseStartup<StartupMultipleAppInsights>());
        }

        [Fact]
        public void ServiceResolution_InvalidInstance()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ArrangeBotFile("multiple_app_insights"); // Default bot file
                ArrangeAppSettings(); // Default app settings
                new TestServer(new WebHostBuilder()
                    .UseStartup<StartupInvalidInstance>());
            });
        }

        /// <summary>
        /// Prepare appsettings.json for test.
        /// </summary>
        /// <remarks>Ensures appsettings.json file is set up (copy based on different sample files,
        /// post-pended with a version.)  ie, appsettings.json.no_app_insights. </remarks>
        /// <param name="version">Post-pended onto the file name to copy (ie, "no_app_insights"). If null, put no file.</param>
        private void ArrangeAppSettings(string version = "default")
        {
            try
            {
                File.Delete("appsettings.json");
            }
            catch
            {
            }

            if (!string.IsNullOrWhiteSpace(version))
            {
                File.Copy($"appsettings.json.{version}", "appsettings.json");
            }
        }

        /// <summary>
        /// Prepare testbot.bot for test.
        /// </summary>
        /// <remarks>Ensures testbot.bot file is set up (copy based on different sample files,
        /// post-pended with a version.)  ie, testbot.bot.no_app_insights. </remarks>
        /// <param name="version">Post-pended onto the file name to copy (ie, "no_app_insights"). If null, put no file.</param>
        private void ArrangeBotFile(string version = "default")
        {
            try
            {
                File.Delete("testbot.bot");
            }
            catch
            {
            }

            if (!string.IsNullOrWhiteSpace(version))
            {
                File.Copy($"testbot.bot.{version}", "testbot.bot");
            }
        }
    }
}
