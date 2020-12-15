// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Bot.Builder.Runtime.Providers;
using Microsoft.Bot.Builder.Runtime.Providers.Adapter;
using Microsoft.Bot.Builder.Runtime.Providers.Credentials;
using Microsoft.Bot.Builder.Runtime.Providers.Storage;
using Microsoft.Bot.Builder.Runtime.Tests.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Extensions
{
    [Collection("ComponentRegistrations")]
    public class ServiceCollectionExtensionTests
    {
        private const string ResourceId = "runtime.json";

        public static IEnumerable<object[]> GetAddBotRuntimeThrowsArgumentNullExceptionData()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();
            Func<IServiceProvider, ResourceExplorer> resourceExplorerImplementationFactory =
                (serviceProvider) => TestDataGenerator.BuildMemoryResourceExplorer();

            yield return new object[]
            {
                "services",
                (IServiceCollection)null,
                configuration,
                resourceExplorerImplementationFactory
            };

            yield return new object[]
            {
                "configuration",
                services,
                (IConfiguration)null,
                resourceExplorerImplementationFactory
            };

            yield return new object[]
            {
                "resourceExplorerImplementationFactory",
                services,
                configuration,
                (Func<IServiceProvider, ResourceExplorer>)null
            };
        }

        [Fact]
        public void AddBotRuntime_Succeeds()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            services.AddBotRuntime(
                configuration,
                (serviceProvider) => TestDataGenerator.BuildMemoryResourceExplorer(new[]
                {
                    new JsonResource(ResourceId, new RuntimeConfigurationProvider
                    {
                        Adapter = new BotCoreAdapterProvider(),
                        Credentials = new DeclarativeCredentialsProvider(),
                        RootDialog = "root.dialog",
                        Storage = new MemoryStorageProvider()
                    })
                }));
        }

        [Theory]
        [MemberData(nameof(GetAddBotRuntimeThrowsArgumentNullExceptionData))]
        public void AddBotRuntime_Throws_ArgumentNullException(
            string paramName,
            IServiceCollection services,
            IConfiguration configuration,
            Func<IServiceProvider, ResourceExplorer> resourceExplorerImplementationFactory)
        {
            Assert.Throws<ArgumentNullException>(
                paramName,
                () => services.AddBotRuntime(configuration, resourceExplorerImplementationFactory));
        }

        [Fact]
        public void AddBotRuntime_Throws_RuntimeConfigurationNotFound()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => services.AddBotRuntime(
                    configuration,
                    (serviceProvider) => TestDataGenerator.BuildMemoryResourceExplorer()));

            Assert.StartsWith(
                expectedStartString: $"Could not find resource '{ResourceId}'",
                actualString: exception.Message);

            Assert.Equal(expected: ResourceId, actual: exception.ParamName);
        }
    }
}
