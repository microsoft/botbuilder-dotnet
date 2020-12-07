// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Runtime.Builders.Transcripts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Builders.Transcripts
{
    public class FileTranscriptLoggerBuilderTests
    {
        public static IEnumerable<object[]> GetBuildSucceedsData()
        {
            yield return new object[]
            {
                (StringExpression)null,
                (BoolExpression)null,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new StringExpression(Environment.CurrentDirectory),
                new BoolExpression(false),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new StringExpression("=folder"),
                new BoolExpression("=unitTestMode"),
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { "folder", Environment.CurrentDirectory },
                    { "unitTestMode", false }
                })
            };
        }

        [Theory]
        [MemberData(nameof(GetBuildSucceedsData))]
        public void Build_Succeeds(StringExpression folder, BoolExpression unitTestMode, IConfiguration configuration)
        {
            IServiceProvider services = new ServiceCollection().BuildServiceProvider();

            ITranscriptLogger transcriptLogger = new FileTranscriptLoggerBuilder
            {
                Folder = folder,
                UnitTestMode = unitTestMode
            }.Build(services, configuration);

            Assert.NotNull(transcriptLogger);
            Assert.IsType<FileTranscriptLogger>(transcriptLogger);
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
                () => new FileTranscriptLoggerBuilder().Build(services, configuration));
        }
    }
}
