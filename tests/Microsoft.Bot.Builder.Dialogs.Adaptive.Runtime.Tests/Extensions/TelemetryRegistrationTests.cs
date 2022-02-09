﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Extensions
{
    public class TelemetryRegistrationTests
    {
        public static IEnumerable<object[]> TelemetryRegistrationTestData()
        {
            yield return new object[]
            {
                null
            };
            yield return new object[]
            {
                new TelemetrySettings()
            };
            yield return new object[]
            {
                new TelemetrySettings()
                {
                    Options = new ApplicationInsightsServiceOptions() { ConnectionString = null }
                }
            };
        }

        [Theory]
        [MemberData(nameof(TelemetryRegistrationTestData))]
        public void AddBotRuntimeTelemetryDisabled(object settings)
        {
            // Setup
            IServiceCollection services = new ServiceCollection();

            var telemetrySettings = settings as TelemetrySettings;
            IConfiguration configuration = new ConfigurationBuilder().AddRuntimeSettings(new RuntimeSettings() { Telemetry = telemetrySettings }).Build();

            // Test
            services.AddBotRuntimeTelemetry(configuration);

            // Assert
            IServiceProvider provider = services.BuildServiceProvider();

            Assertions.AssertService<IBotTelemetryClient, NullBotTelemetryClient>(services, provider, ServiceLifetime.Singleton);
        }

        [Fact]
        public void AddBotRuntimeTelemetryEnabled()
        {
            // Setup
            IServiceCollection services = new ServiceCollection();

            var telemetrySettings = new TelemetrySettings() { Options = new ApplicationInsightsServiceOptions() { ConnectionString = Guid.NewGuid().ToString() } };
            IConfiguration configuration = new ConfigurationBuilder().AddRuntimeSettings(new RuntimeSettings() { Telemetry = telemetrySettings }).Build();

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IHostingEnvironment, TestHostingEnvironment>();

            // Test
            services.AddBotRuntimeTelemetry(configuration);
            
            // Assert
            IServiceProvider provider = services.BuildServiceProvider();

            Assertions.AssertService<IMiddleware, TelemetryInitializerMiddleware>(services, provider, ServiceLifetime.Singleton);

            Assertions.AssertService<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>(
                services,
                provider,
                ServiceLifetime.Singleton,
                searchOptions: ServiceDescriptorSearchOptions
                    .SearchByImplementationType<OperationCorrelationTelemetryInitializer>());

            Assertions.AssertService<ITelemetryInitializer, TelemetryBotIdInitializer>(
                services,
                provider,
                ServiceLifetime.Singleton,
                searchOptions: ServiceDescriptorSearchOptions
                    .SearchByImplementationType<TelemetryBotIdInitializer>());

            Assertions.AssertService<IBotTelemetryClient, BotTelemetryClient>(
                services,
                provider,
                ServiceLifetime.Singleton);
        }
    }
}
