// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Runtime.Builders.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Builders.Middleware
{
    public class InspectionMiddlewareBuilderTests
    {
        [Fact]
        public void Build_Succeeds()
        {
            IServiceProvider services = new ServiceCollection()
                .AddTransient<IStorage>(_ => new MemoryStorage())
                .BuildServiceProvider();

            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            IMiddleware middleware = new InspectionMiddlewareBuilder().Build(services, configuration);

            Assert.NotNull(middleware);
            Assert.IsType<InspectionMiddleware>(middleware);
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
                () => new InspectionMiddlewareBuilder().Build(services, configuration));
        }

        [Fact]
        public void Build_Throws_StorageNotRegistered()
        {
            IServiceProvider services = new ServiceCollection().BuildServiceProvider();

            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            Assert.Throws<ArgumentNullException>(
                "storage",
                () => new InspectionMiddlewareBuilder().Build(services, configuration));
        }

        [Fact]
        public void Build_Throws_StorageNull()
        {
            IServiceProvider services = new ServiceCollection()
                .AddTransient<IStorage>(_ => null)
                .BuildServiceProvider();

            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            Assert.Throws<ArgumentNullException>(
                "storage",
                () => new InspectionMiddlewareBuilder().Build(services, configuration));
        }
    }
}
