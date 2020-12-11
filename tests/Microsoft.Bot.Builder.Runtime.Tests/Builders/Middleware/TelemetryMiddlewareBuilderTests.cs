// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Properties;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Runtime.Builders.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Builders.Middleware
{
    public class TelemetryMiddlewareBuilderTests
    {
        public static IEnumerable<object[]> GetBuildSucceedsData()
        {
            yield return new object[]
            {
                (BoolExpression)null,
                (BoolExpression)null,
                (IBotTelemetryClient)null,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new BoolExpression(false),
                new BoolExpression(true),
                (IBotTelemetryClient)new NullBotTelemetryClient(),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new BoolExpression("=logActivities"),
                new BoolExpression("=logPersonalInformation"),
                (IBotTelemetryClient)new NullBotTelemetryClient(),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { "logActivities", false },
                    { "logPersonalInformation", true }
                })
            };
        }

        [Theory]
        [MemberData(nameof(GetBuildSucceedsData))]
        public void Build_Succeeds(
            BoolExpression logActivities,
            BoolExpression logPersonalInformation,
            IBotTelemetryClient botTelemetryClient,
            IConfiguration configuration)
        {
            IServiceProvider services = new ServiceCollection()
                .AddTransient<IBotTelemetryClient>(_ => botTelemetryClient)
                .AddTransient<IHttpContextAccessor, HttpContextAccessor>()
                .BuildServiceProvider();

            IMiddleware middleware = new TelemetryMiddlewareBuilder
            {
                LogActivities = logActivities,
                LogPersonalInformation = logPersonalInformation
            }.Build(services, configuration);

            Assert.NotNull(middleware);
            Assert.IsType<TelemetryInitializerMiddleware>(middleware);
        }

        [Theory]
        [MemberData(
            nameof(BuilderTestDataGenerator.GetBuildArgumentNullExceptionData),
            MemberType = typeof(BuilderTestDataGenerator))]
        public void Build_Throws_ArgumentNullException(
            string paramName,
            IServiceProvider services,
            IConfiguration configuration)
        {
            Assert.Throws<ArgumentNullException>(
                paramName,
                () => new TelemetryMiddlewareBuilder().Build(services, configuration));
        }
    }
}
