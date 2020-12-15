// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Runtime.Builders.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Builders.Middleware
{
    public class ShowTypingMiddlewareBuilderTests
    {
        public static IEnumerable<object[]> GetBuildSucceedsData()
        {
            yield return new object[]
            {
                (IntExpression)null,
                (IntExpression)null,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new IntExpression(1),
                new IntExpression(1),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new IntExpression("=delay"),
                new IntExpression("=period"),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { "delay", 1 },
                    { "period", 1 }
                })
            };
        }

        [Theory]
        [MemberData(nameof(GetBuildSucceedsData))]
        public void Build_Succeeds(IntExpression delay, IntExpression period, IConfiguration configuration)
        {
            IServiceProvider services = new ServiceCollection().BuildServiceProvider();

            IMiddleware middleware = new ShowTypingMiddlewareBuilder
            {
                Delay = delay,
                Period = period
            }.Build(services, configuration);

            Assert.NotNull(middleware);
            Assert.IsType<ShowTypingMiddleware>(middleware);
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
                () => new ShowTypingMiddlewareBuilder().Build(services, configuration));
        }

        [Theory]
        [InlineData("delay", -1, 1)]
        [InlineData("period", 0, 0)]
        [InlineData("period", 0, -1)]
        public void Build_Throws_ArgumentOutOfRangeException(string paramName, int delay, int period)
        {
            IServiceProvider services = new ServiceCollection().BuildServiceProvider();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            Assert.Throws<ArgumentOutOfRangeException>(
                paramName,
                () => new ShowTypingMiddlewareBuilder
                {
                    Delay = new IntExpression(delay),
                    Period = new IntExpression(period)
                }.Build(services, configuration));
        }
    }
}
