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

namespace Microsoft.Bot.ApplicationInsights.WebApi.Tests
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
                var server = new TestServer(new WebHostBuilder()
                    .UseStartup<Startup>());
            }
        }

        }
    }
