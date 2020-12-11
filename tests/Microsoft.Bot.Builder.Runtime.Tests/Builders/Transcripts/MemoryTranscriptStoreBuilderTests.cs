// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Runtime.Builders.Transcripts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Builders.Transcripts
{
    public class MemoryTranscriptStoreBuilderTests
    {
        [Fact]
        public void Build_Succeeds()
        {
            IServiceProvider services = new ServiceCollection().BuildServiceProvider();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            ITranscriptLogger transcriptLogger = new MemoryTranscriptStoreBuilder().Build(services, configuration);

            Assert.NotNull(transcriptLogger);
            Assert.IsType<MemoryTranscriptStore>(transcriptLogger);
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
                () => new MemoryTranscriptStoreBuilder().Build(services, configuration));
        }
    }
}
