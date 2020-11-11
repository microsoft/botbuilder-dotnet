// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Properties;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
#if NETCOREAPP2_1
using Microsoft.AspNetCore.Hosting.Internal;
#endif
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Runtime.Providers.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Providers.Telemetry
{
    public class ApplicationInsightsTelemetryProviderTests
    {
        public static IEnumerable<object[]> GetConfigureServicesSucceedsData()
        {
            string instrumentationKey = Guid.NewGuid().ToString();

            yield return new object[]
            {
                (StringExpression)null,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new StringExpression((string)null),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new StringExpression(string.Empty),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new StringExpression(instrumentationKey),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new StringExpression("=instrumentationKey"),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { "instrumentationKey", instrumentationKey }
                })
            };
        }

        [Theory]
        [MemberData(nameof(GetConfigureServicesSucceedsData))]
        public void ConfigureServices_Succeeds(
            StringExpression instrumentationKey,
            IConfiguration configuration)
        {
            var services = new ServiceCollection();

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
#if NETCOREAPP2_1
            services.AddTransient<IHostingEnvironment, HostingEnvironment>();
#elif NETCOREAPP3_1
            services.AddTransient<IHostingEnvironment, TestHostingEnvironment>();
#endif

            new ApplicationInsightsTelemetryProvider
            {
                InstrumentationKey = instrumentationKey
            }.ConfigureServices(services, configuration);

            IServiceProvider provider = services.BuildServiceProvider();

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

        [Theory]
        [MemberData(
            nameof(ProviderTestDataGenerator.GetConfigureServicesArgumentNullExceptionData),
            MemberType = typeof(ProviderTestDataGenerator))]
        public void ConfigureServices_Throws_ArgumentNullException(
            string paramName,
            IServiceCollection services,
            IConfiguration configuration)
        {
            Assert.Throws<ArgumentNullException>(
                paramName,
                () => new ApplicationInsightsTelemetryProvider().ConfigureServices(services, configuration));
        }
    }
}
