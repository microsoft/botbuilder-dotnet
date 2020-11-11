// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Runtime.Providers.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Providers.Storage
{
    public class CosmosDbPartitionedStorageProviderTests
    {
        public static IEnumerable<object[]> GetConfigureServicesSucceedsData()
        {
            string authenticationKey = Guid.NewGuid().ToString();
            bool compatibilityMode = false;
            string containerId = Guid.NewGuid().ToString();
            int containerThroughput = 1000;
            string databaseId = Guid.NewGuid().ToString();
            string endpoint = Guid.NewGuid().ToString();
            string keySuffix = Guid.NewGuid().ToString();

            yield return new object[]
            {
                new StringExpression(authenticationKey),
                (BoolExpression)null,
                new StringExpression(containerId),
                (IntExpression)null,
                new StringExpression(databaseId),
                new StringExpression(endpoint),
                (StringExpression)null,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new StringExpression(authenticationKey),
                new BoolExpression(compatibilityMode),
                new StringExpression(containerId),
                new IntExpression(containerThroughput),
                new StringExpression(databaseId),
                new StringExpression(endpoint),
                new StringExpression(keySuffix),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new StringExpression("=authenticationKey"),
                new BoolExpression("=compatibilityMode"),
                new StringExpression("=containerId"),
                new IntExpression("=containerThroughput"),
                new StringExpression("=databaseId"),
                new StringExpression("=endpoint"),
                new StringExpression("=keySuffix"),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(
                    new JObject
                    {
                        { "authenticationKey", authenticationKey },
                        { "compatibilityMode", compatibilityMode },
                        { "containerId", containerId },
                        { "containerThroughput", containerThroughput },
                        { "databaseId", databaseId },
                        { "endpoint", endpoint },
                        { "keySuffix", keySuffix }
                    })
            };
        }

        [Theory]
        [MemberData(nameof(GetConfigureServicesSucceedsData))]
        public void ConfigureServices_Succeeds(
            StringExpression authenticationKey,
            BoolExpression compatibilityMode,
            StringExpression containerId,
            IntExpression containerThroughput,
            StringExpression databaseId,
            StringExpression endpoint,
            StringExpression keySuffix,
            IConfiguration configuration)
        {
            var services = new ServiceCollection();

            new CosmosDbPartitionedStorageProvider
            {
                AuthenticationKey = authenticationKey,
                CompatibilityMode = compatibilityMode,
                ContainerId = containerId,
                ContainerThroughput = containerThroughput,
                DatabaseId = databaseId,
                Endpoint = endpoint,
                KeySuffix = keySuffix
            }.ConfigureServices(services, configuration);

            IServiceProvider provider = services.BuildServiceProvider();

            Assertions.AssertService<IStorage, CosmosDbPartitionedStorage>(
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
                () => new CosmosDbPartitionedStorageProvider().ConfigureServices(services, configuration));
        }

        [Theory]
        [InlineData((string)null, true, "containerId", "databaseId", "endpoint", (string)null)]
        [InlineData("", true, "containerId", "databaseId", "endpoint", (string)null)]
        [InlineData("authKey", true, (string)null, "databaseId", "endpoint", (string)null)]
        [InlineData("authKey", true, "", "databaseId", "endpoint", (string)null)]
        [InlineData("authKey", true, "containerId", (string)null, "endpoint", (string)null)]
        [InlineData("authKey", true, "containerId", "", "endpoint", (string)null)]
        [InlineData("authKey", true, "containerId", "databaseId", (string)null, (string)null)]
        [InlineData("authKey", true, "containerId", "databaseId", "endpoint", "\\?/#*")]
        [InlineData("authKey", false, "containerId", "databaseId", "endpoint", "\\?/#*")]
        public void ConfigureServices_Throws_CosmosDbStorageOptionsArgumentException(
            string authenticationKey,
            bool compatibilityMode,
            string containerId,
            string databaseId,
            string endpoint,
            string keySuffix)
        {
            var services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            new CosmosDbPartitionedStorageProvider
            {
                AuthenticationKey = new StringExpression(authenticationKey),
                CompatibilityMode = new BoolExpression(compatibilityMode),
                ContainerId = new StringExpression(containerId),
                DatabaseId = new StringExpression(databaseId),
                Endpoint = new StringExpression(endpoint),
                KeySuffix = new StringExpression(keySuffix)
            }.ConfigureServices(services, configuration);

            IServiceProvider provider = services.BuildServiceProvider();

            Assertions.AssertServiceThrows<IStorage, CosmosDbPartitionedStorage, ArgumentException>(
                services,
                provider,
                ServiceLifetime.Singleton,
                assert: (exception) =>
                {
                    Assert.Equal("cosmosDbStorageOptions", exception.ParamName);
                });
        }
    }
}
