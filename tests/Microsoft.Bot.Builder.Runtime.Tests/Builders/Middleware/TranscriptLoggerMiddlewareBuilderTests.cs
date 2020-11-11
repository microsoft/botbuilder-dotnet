// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Runtime.Builders.Middleware;
using Microsoft.Bot.Builder.Runtime.Builders.Transcripts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Builders.Middleware
{
    public class TranscriptLoggerMiddlewareBuilderTests
    {
        [Fact]
        public void Build_Succeeds()
        {
            IServiceProvider services = new ServiceCollection().BuildServiceProvider();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            IMiddleware middleware = new TranscriptLoggerMiddlewareBuilder
            {
                TranscriptStore = new MemoryTranscriptStoreBuilder()
            }.Build(services, configuration);

            Assert.NotNull(middleware);
            Assert.IsType<TranscriptLoggerMiddleware>(middleware);
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
                () => new TranscriptLoggerMiddlewareBuilder().Build(services, configuration));
        }

        [Fact]
        public void Build_Throws_TranscriptLoggerNull()
        {
            IServiceProvider services = new ServiceCollection().BuildServiceProvider();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            Assert.Throws<ArgumentNullException>(
                "transcriptLogger",
                () => new TranscriptLoggerMiddlewareBuilder().Build(services, configuration));
        }
    }
}
