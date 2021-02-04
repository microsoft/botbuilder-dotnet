// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Extensions
{
    [Collection("ComponentRegistrations")]
    public class ServiceCollectionExtensionTests
    {
        public static IEnumerable<object[]> GetAddBotRuntimeThrowsArgumentNullExceptionData()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            yield return new object[]
            {
                "services",
                (IServiceCollection)null,
                configuration
            };

            yield return new object[]
            {
                "configuration",
                services,
                (IConfiguration)null
            };
        }

        [Fact]
        public void AddBotRuntime_Succeeds()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            services.AddSingleton(TestDataGenerator.BuildMemoryResourceExplorer());
            services.AddBotRuntime(configuration);
        }

        [Theory]
        [MemberData(nameof(GetAddBotRuntimeThrowsArgumentNullExceptionData))]
        public void AddBotRuntime_Throws_ArgumentNullException(string paramName, IServiceCollection services, IConfiguration configuration)
        {
            Assert.Throws<ArgumentNullException>(
                paramName,
                () => services.AddBotRuntime(configuration));
        }
    }
}
