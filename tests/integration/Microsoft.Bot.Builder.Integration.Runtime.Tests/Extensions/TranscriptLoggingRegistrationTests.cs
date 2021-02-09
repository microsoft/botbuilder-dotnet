// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Bot.Builder.Integration.Runtime.Extensions;
using Microsoft.Bot.Builder.Integration.Runtime.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Extensions
{
    public class TranscriptLoggingRegistrationTests
    {
        private const string LocalConnectionString = "UseDevelopmentStorage=true";
        private const string LocalContainer = "testContainer";
        private static readonly BlobsStorageSettings LocalBlobSettings = new BlobsStorageSettings() { ConnectionString = LocalConnectionString, ContainerName = LocalContainer };

        public static IEnumerable<object[]> GetAddBotRuntimeTranscriptLoggerData()
        {
            var settings = new Dictionary<string, string>
            {
                { "blobTranscript:connectionString", LocalConnectionString },
                { "blobTranscript:containerName", "containerName" },
            };

            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            // params: IConfiguration configuration, FeatureSettings settings, int expectedRegistrationCount
            yield return new object[]
            {
                configuration,
                new FeatureSettings() { TraceTranscript = true, BlobTranscript = LocalBlobSettings },
                2
            };
            yield return new object[]
            {
                configuration,
                new FeatureSettings() { TraceTranscript = true, BlobTranscript = null },
                1
            };
            yield return new object[]
            {
                configuration,
                new FeatureSettings() { TraceTranscript = false, BlobTranscript = LocalBlobSettings },
                1
            };
            yield return new object[]
            {
                configuration,
                new FeatureSettings() { TraceTranscript = false, BlobTranscript = null },
                0
            };
        }

        public static IEnumerable<object[]> GetAddBotRuntimeTranscriptLoggerErrorData()
        {
            // params: string connectionString, string containerName
            yield return new object[] { "connectionString", null };
            yield return new object[] { null, "containerName" };
            yield return new object[] { null, null };
            yield return new object[] { string.Empty, "containerName" };
            yield return new object[] { "containerName", string.Empty };
        }

        [Theory]
        [MemberData(nameof(GetAddBotRuntimeTranscriptLoggerErrorData))]
        public void AddBotRuntimeTranscriptLogger_ErrorCases(string connectionString, string containerNAme)
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().Build();
            var featureSettings = new FeatureSettings() { BlobTranscript = new BlobsStorageSettings() { ConnectionString = connectionString, ContainerName = containerNAme } };
            Assert.Throws<ArgumentNullException>(() => services.AddBotRuntimeTranscriptLogging(configuration, featureSettings));
        }

        [Fact]
        public void AddBotRuntimeTranscriptLogger_NullConfiguration_Throws()
        {
            IServiceCollection services = new ServiceCollection();
            var featureSettings = new FeatureSettings() { BlobTranscript = LocalBlobSettings };
            Assert.Throws<ArgumentNullException>(() => services.AddBotRuntimeTranscriptLogging(null, featureSettings));
        }

        [Theory]
        [MemberData(nameof(GetAddBotRuntimeTranscriptLoggerData))]
        public void AddBotRuntimeTranscriptLogger(IConfiguration configuration, object settings, int middlewareCount)
        {
            IServiceCollection services = new ServiceCollection();
            var featureSettings = settings as FeatureSettings;

            services.AddBotRuntimeTranscriptLogging(configuration, featureSettings);

            var serviceProvider = services.BuildServiceProvider();

            var registeredServices = serviceProvider.GetServices<IMiddleware>();
            Assert.Equal(middlewareCount, registeredServices.Count());

            foreach (var service in registeredServices)
            {
                Assert.IsType<TranscriptLoggerMiddleware>(service);
            }
        }
    }
}
