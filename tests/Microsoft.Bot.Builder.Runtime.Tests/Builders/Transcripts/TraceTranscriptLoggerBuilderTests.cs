// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Runtime.Builders.Transcripts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Builders.Transcripts
{
    public class TraceTranscriptLoggerBuilderTests
    {
        public static IEnumerable<object[]> GetBuildSucceedsData()
        {
            yield return new object[]
            {
                (BoolExpression)null,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new BoolExpression(false),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new BoolExpression("=traceActivity"),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { "traceActivity", false }
                })
            };
        }

        [Theory]
        [MemberData(nameof(GetBuildSucceedsData))]
        public void Build_Succeeds(BoolExpression traceActivity, IConfiguration configuration)
        {
            IServiceProvider services = new ServiceCollection().BuildServiceProvider();

            ITranscriptLogger transcriptLogger = new TraceTranscriptLoggerBuilder
            {
                TraceActivity = traceActivity
            }.Build(services, configuration);

            Assert.NotNull(transcriptLogger);
            Assert.IsType<TraceTranscriptLogger>(transcriptLogger);
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
                () => new TraceTranscriptLoggerBuilder().Build(services, configuration));
        }
    }
}
