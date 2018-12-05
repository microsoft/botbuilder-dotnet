// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.TestHost;
using System.IO;
using Microsoft.ApplicationInsights;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Core.Tests
{
    [TestClass]
    [TestCategory("ApplicationInsights")]
    public class ServiceResolutionTests
    {
        public ServiceResolutionTests()
        {
            // Arrange
            //_server = new TestServer(new WebHostBuilder()
                                     //.UseStartup<Startup>());
            //_client = _server.CreateClient();
        }
        [TestMethod]
        public void AppSettings_NoAppSettings()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings(null); // No appsettings file
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());

            // Telemetry Client should be active, just not configured.
            // This is not an error condition so samples can degrade.
            var telemetryClient = new TelemetryClient();
            Assert.IsTrue(string.IsNullOrWhiteSpace(telemetryClient.InstrumentationKey));
        }
        [TestMethod]
        public void AppSettings_NoAppInsights()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings("no_app_insights"); // Bad app insights appsettings file

            var server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            // Telemetry Client should be active, just not configured.
            // This is not an error condition so samples can degrade.
            var telemetryClient = new TelemetryClient();
            Assert.IsTrue(string.IsNullOrWhiteSpace(telemetryClient.InstrumentationKey));
        }

        [TestMethod]
        public void AppSettings_NoAppInsightsKey()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings("no_instrumentation_key"); // Bad app insights appsettings file

            var server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());

            // Telemetry Client should be active, just not configured.
            // This is not an error condition so samples can degrade.
            var telemetryClient = new TelemetryClient();
            Assert.IsTrue(string.IsNullOrWhiteSpace(telemetryClient.InstrumentationKey));
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void BotFile_NoBotFile()
        {
            ArrangeBotFile(null); // No bot file
            ArrangeAppSettings(); // Default app settings
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
        }

        [TestMethod]
        public void BotFile_NoAppInsightsInBot()
        {
            // Should default to the Null TelemetryClient
            ArrangeBotFile("no_app_insights"); // Invalid bot file
            ArrangeAppSettings(); // Default app settings
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<StartupVerifyNullTelemetry>());
        }

        [TestMethod]
        public void ServiceResolution_GoodLoad()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings(); // Default app settings
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            Assert.IsTrue(true);
        }


        [TestMethod]
        public void ServiceResolution_VerifyTelemetryClient()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings(); // Default app settings
            var server = new TestServer(new WebHostBuilder()
                .UseApplicationInsights()
                .UseStartup<StartupVerifyTelemetryClient>());
        }


        [TestMethod]
        public void ServiceResolution_OverrideTelemetry()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings(); // Default app settings
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<StartupOverideTelemClient>());
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ServiceResolution_MultipleAppInsights()
        {
            ArrangeBotFile("multiple_app_insights"); // Default bot file
            ArrangeAppSettings(); // Default app settings
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<StartupMultipleAppInsights>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ServiceResolution_InvalidInstance()
        {
            ArrangeBotFile("multiple_app_insights"); // Default bot file
            ArrangeAppSettings(); // Default app settings
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<StartupInvalidInstance>());
        }

        /// <summary>
        /// Prepare appsettings.json for test
        /// </summary>
        /// <remarks>Ensures appsettings.json file is set up (copy based on different sample files,
        /// post-pended with a version.)  ie, appsettings.json.no_app_insights </remarks>
        /// <param name="version">Post-pended onto the file name to copy (ie, "no_app_insights"). If null, put no file.</param>
        public void ArrangeAppSettings(string version = "default")
        {
            try { File.Delete("appsettings.json"); }
            catch { }
            
            if (!string.IsNullOrWhiteSpace(version))
            {
                File.Copy($"appsettings.json.{version}", "appsettings.json");
            }
        }
        /// <summary>
        /// Prepare testbot.bot for test
        /// </summary>
        /// <remarks>Ensures testbot.bot file is set up (copy based on different sample files,
        /// post-pended with a version.)  ie, testbot.bot.no_app_insights </remarks>
        /// <param name="version">Post-pended onto the file name to copy (ie, "no_app_insights"). If null, put no file.</param>

        public void ArrangeBotFile(string version = "default")
        {
            try { File.Delete("testbot.bot"); }
            catch { }
            
            if (!string.IsNullOrWhiteSpace(version))
            {
                File.Copy($"testbot.bot.{version}", "testbot.bot");
            }
        }

    }
}
