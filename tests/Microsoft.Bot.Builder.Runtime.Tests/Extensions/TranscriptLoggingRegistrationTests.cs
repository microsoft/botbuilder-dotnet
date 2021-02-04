// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Bot.Builder.Runtime.Settings;
using Microsoft.Bot.Builder.Runtime.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Extensions
{
    public class TranscriptLoggingRegistrationTests
    {
        public static IEnumerable<object[]> GetAddBotRuntimeTranscriptLoggerData()
        {
            var settings = new Dictionary<string, string>
            {
                { "blobTranscript:connectionString", "UseDevelopmentStorage=true" },
                { "blobTranscript:containerName", "containerName" },
            };

            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            // params: IConfiguration configuration, FeatureSettings settings, int expectedRegistrationCount
            yield return new object[]
            {
                configuration,
                new FeatureSettings() { TraceTranscript = true, BlobTranscript = true },
                2
            };
            yield return new object[]
            {
                configuration,
                new FeatureSettings() { TraceTranscript = true, BlobTranscript = false },
                1
            };
            yield return new object[]
            {
                configuration,
                new FeatureSettings() { TraceTranscript = false, BlobTranscript = true },
                1
            };
            yield return new object[]
            {
                configuration,
                new FeatureSettings() { TraceTranscript = false, BlobTranscript = false },
                0
            };
        }

        public static IEnumerable<object[]> GetAddBotRuntimeTranscriptLoggerErrorData()
        {
            // params: config settings dictionary
            yield return new object[]
            {
                new Dictionary<string, string>
                {
                    { "blobTranscript:connectionStringWRONG", "connectionString" },
                    { "blobTranscript:containerName", "containerName" },
                }
            };
            yield return new object[]
            {
                new Dictionary<string, string>
                {
                    { "blobTranscript:connectionString", "connectionString" },
                }
            };
            yield return new object[]
            {
                new Dictionary<string, string>
                {
                    { "blobTranscript:containerName", "containerName" },
                }
            };
            yield return new object[]
            {
                new Dictionary<string, string>()
            };
        }

        [Theory]
        [MemberData(nameof(GetAddBotRuntimeTranscriptLoggerErrorData))]
        public void AddBotRuntimeTranscriptLogger_ErrorCases(Dictionary<string, string> settings)
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            var featureSettings = new FeatureSettings() { BlobTranscript = true };
            Assert.Throws<ConfigurationException>(() => services.AddBotRuntimeTranscriptLogging(configuration, featureSettings));
        }

        [Fact]
        public void AddBotRuntimeTranscriptLogger_NullConfiguration_Throws()
        {
            IServiceCollection services = new ServiceCollection();
            var featureSettings = new FeatureSettings() { BlobTranscript = true };
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
