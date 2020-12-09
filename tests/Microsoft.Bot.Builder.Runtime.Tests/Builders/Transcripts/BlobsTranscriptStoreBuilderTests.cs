// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Runtime.Builders.Transcripts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Builders.Transcripts
{
    public class BlobsTranscriptStoreBuilderTests
    {
        private const string ConnectionString = "UseDevelopmentStorage=true";

        public static IEnumerable<object[]> GetBuildSucceedsData()
        {
            yield return new object[]
            {
                new StringExpression(ConnectionString),
                new StringExpression("container-name"),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new StringExpression("=connectionString"),
                new StringExpression("=containerName"),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { "connectionString", ConnectionString },
                    { "containerName", "container-name" }
                })
            };
        }

        [Theory]
        [MemberData(nameof(GetBuildSucceedsData))]
        public void Build_Succeeds(
            StringExpression connectionString,
            StringExpression containerName,
            IConfiguration configuration)
        {
            IServiceProvider services = new ServiceCollection().BuildServiceProvider();

            ITranscriptLogger transcriptLogger = new BlobsTranscriptStoreBuilder
            {
                ConnectionString = connectionString,
                ContainerName = containerName
            }.Build(services, configuration);

            Assert.NotNull(transcriptLogger);
            Assert.IsType<BlobsTranscriptStore>(transcriptLogger);
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
                () => new BlobsTranscriptStoreBuilder().Build(services, configuration));
        }

        [Theory]
        [InlineData((string)null)]
        [InlineData((string)"")]
        public void Build_Throws_ConnectionStringNullOrEmpty(string connectionString)
        {
            IServiceProvider services = new ServiceCollection().BuildServiceProvider();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            Assert.Throws<ArgumentNullException>(
                "dataConnectionString",
                () => new BlobsTranscriptStoreBuilder
                {
                    ConnectionString = new StringExpression(connectionString),
                    ContainerName = new StringExpression("container-name")
                }.Build(services, configuration));
        }

        [Theory]
        [InlineData((string)null)]
        [InlineData((string)"")]
        public void Build_Throws_ContainerNameNullOrEmpty(string containerName)
        {
            IServiceProvider services = new ServiceCollection().BuildServiceProvider();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            Assert.Throws<ArgumentNullException>(
                "containerName",
                () => new BlobsTranscriptStoreBuilder
                {
                    ConnectionString = new StringExpression(ConnectionString),
                    ContainerName = new StringExpression(containerName)
                }.Build(services, configuration));
        }
    }
}
