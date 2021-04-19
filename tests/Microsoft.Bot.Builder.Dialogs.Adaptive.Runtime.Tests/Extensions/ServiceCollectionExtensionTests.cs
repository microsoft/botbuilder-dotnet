// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Extensions
{
    [Collection("ComponentRegistrations")]
    public class ServiceCollectionExtensionTests
    {
        public static IEnumerable<object[]> GetAddBotRuntimeThrowsArgumentNullExceptionData()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().Build();

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
            IConfiguration configuration = new ConfigurationBuilder().Build();

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
