// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.BotBuilderSamples.Tests.Framework.Luis;
using Microsoft.BotBuilderSamples.Tests.Framework.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.BotBuilderSamples.Tests.CognitiveModels
{
    public class FlightBookingLuisTests
    {
        private const string RelativePath = @"CognitiveModels/Data";

        private static readonly Lazy<LuisRecognizer> _luisRecognizerLazy = new Lazy<LuisRecognizer>(() =>
        {
            var configuration = TestConfiguration.Instance.Configuration;

            // Create LuisRecognizer instance
            var luisApplication = new LuisApplication(
                configuration.GetSection("cognitiveModels:flightBooking:luisAppId").Value,
                configuration.GetSection("cognitiveModels:flightBooking:luisEndpointKey").Value,
                configuration.GetSection("cognitiveModels:flightBooking:luisEndpoint").Value);

            // Create Recognizer instance
            return new LuisRecognizer(luisApplication, null, false, null);
        });

        private readonly ITestOutputHelper _output;

        public FlightBookingLuisTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void AnalyticThresholds()
        {
            var analyzer = new LuisAnalyzer();
            var sut = analyzer.Analyze("flightbooking.lu");

            Assert.True(sut.IncorrectPredictions.Count == 0);
            Assert.True(sut.ImbalancedIntents.Count == 0);
            Assert.True(sut.UnclearPredictions.Count == 0);
        }

        [Theory]
        [LuDownData(@"flightBookingTestSet.lu", RelativePath)]
        public async Task FlightBookingTestSet(TestDataObject luisData)
        {
            var luisTestItem = luisData.GetObject<LuisTestItem>();

            _output.WriteLine($"Utterance: {luisTestItem.Utterance}");
            _output.WriteLine("\r\nExpected:");
            _output.WriteAsFormattedJson(luisTestItem);
            var luisResult = await _luisRecognizerLazy.Value.RecognizeAsync(luisTestItem.Utterance, CancellationToken.None);

            _output.WriteLine("\r\nActual:");
            _output.WriteAsFormattedJson(luisResult);

            // Assert intent
            Assert.Equal(luisTestItem.ExpectedIntent, luisResult.GetTopScoringIntent().intent);

            // Assert entities
            var resultEntities = luisResult.Entities["$instance"];
            foreach (var entity in luisTestItem.ExpectedEntities)
            {
                // assert the entity is there
                var entityValues = resultEntities[entity.Entity];
                Assert.True(entityValues != null, $"{entity.Entity} found in results");

                // assert the value for the entity matches.
                var expectedEntityValue = luisTestItem.Utterance.Substring(entity.StartPos, entity.EndPos - entity.StartPos + 1);

                Assert.Equal(expectedEntityValue, entityValues.FirstOrDefault()?["text"].ToString());
            }
        }
    }
}
