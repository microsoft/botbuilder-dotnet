// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Moq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Bot.Builder.ApplicationInsights.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using System.IO;

namespace Microsoft.Bot.Builder.ApplicationInsights.Core.Tests
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void AppSettings_NoAppSettings()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings(null); // No appsettings file
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
        }
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AppSettings_NoAppInsights()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings("no_app_insights"); // Bad app insights appsettings file

            var server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AppSettings_NoAppInsightsKey()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings("no_instrumentation_key"); // Bad app insights appsettings file

            var server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void BotFile_NoAppInsights()
        {
            ArrangeBotFile("no_app_insights"); // Invalid bot file
            ArrangeAppSettings(); // Default app settings
            var server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void ServiceResolution_VerifyTelemetryClientFail()
        {
            ArrangeBotFile(); // Default bot file
            ArrangeAppSettings(); // Default app settings
            var server = new TestServer(new WebHostBuilder() // No App Insights registered!
                .UseStartup<StartupVerifyTelemetryClient>());
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
