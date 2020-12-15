// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Runtime.Providers.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Providers.Storage
{
    public class BlobStorageProviderTests
    {
        private const string ConnectionString = "UseDevelopmentStorage=true";

        public static IEnumerable<object[]> GetConfigureServicesSucceedsData()
        {
            string containerName = Guid.NewGuid().ToString();

            yield return new object[]
            {
                new StringExpression(ConnectionString),
                new StringExpression(containerName),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new StringExpression("=connectionString"),
                new StringExpression("=containerName"),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { "connectionString", ConnectionString },
                    { "containerName", containerName }
                })
            };
        }

        [Theory]
        [MemberData(nameof(GetConfigureServicesSucceedsData))]
        public void ConfigureServices_Succeeds(
            StringExpression connectionString,
            StringExpression containerName,
            IConfiguration configuration)
        {
            var services = new ServiceCollection();

            new BlobStorageProvider
            {
                ConnectionString = connectionString,
                ContainerName = containerName
            }.ConfigureServices(services, configuration);

            IServiceProvider provider = services.BuildServiceProvider();

            Assertions.AssertService<IStorage, BlobsStorage>(
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
                () => new BlobStorageProvider().ConfigureServices(services, configuration));
        }

        [Fact]
        public void Build_Throws_ConnectionStringFormatInvalid()
        {
            var services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            Assert.Throws<FormatException>(
                () => new BlobStorageProvider
                {
                    ConnectionString = new StringExpression("InvalidConnectionString"),
                    ContainerName = new StringExpression("container-name")
                }.ConfigureServices(services, configuration));
        }

        [Theory]
        [InlineData((string)null)]
        [InlineData((string)"")]
        public void Build_Throws_ConnectionStringNullOrEmpty(string connectionString)
        {
            var services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            Assert.Throws<ArgumentNullException>(
                "dataConnectionString",
                () => new BlobStorageProvider
                {
                    ConnectionString = new StringExpression(connectionString),
                    ContainerName = new StringExpression("container-name")
                }.ConfigureServices(services, configuration));
        }

        [Fact]
        public void Build_Throws_ContainerNameEmpty()
        {
            var services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            Assert.Throws<ArgumentNullException>(
                () => new BlobStorageProvider
                {
                    ConnectionString = new StringExpression(ConnectionString),
                    ContainerName = new StringExpression(string.Empty)
                }.ConfigureServices(services, configuration));
        }

        [Fact]
        public void Build_Throws_ContainerNameNull()
        {
            var services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            Assert.Throws<ArgumentNullException>(
                "containerName",
                () => new BlobStorageProvider
                {
                    ConnectionString = new StringExpression(ConnectionString),
                    ContainerName = new StringExpression((string)null)
                }.ConfigureServices(services, configuration));
        }
    }
}
