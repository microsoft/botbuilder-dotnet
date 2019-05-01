// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.BotBuilderSamples.Tests.Extensions;
using Microsoft.BotBuilderSamples.Tests.Utils;
using Microsoft.BotBuilderSamples.Tests.Utils.Luis;
using Microsoft.BotBuilderSamples.Tests.Utils.XUnit;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.BotBuilderSamples.Tests.CognitiveModels
{
    // Scenarios
    // intent, entity, utterance, role, version.
    // given a lu file for a LUIS model ensure that the labeled utterances and entities resolve as expected
    // given a json file and a LUIS model, I would like to be able to ensure the utterances and entities defined in the json file resolve as expected
    // given a set of utterances, I would like to assert that they map to the expected intent
    //
    // Assumptions
    // assume local LUIS in container or full model somewhere
    // pure xunit Theories
    // LUDown files as source
    public class CalendarLuisTests : IClassFixture<CalendarLuisTests.LuisTesterFixture>
    {
        private readonly LuisTesterFixture _luisTester;
        private readonly ITestOutputHelper _output;

        public CalendarLuisTests(ITestOutputHelper output, LuisTesterFixture luisTester)
        {
            _luisTester = luisTester;
            _output = output;
        }

        [Theory]
        [FileData(typeof(LuDownDataGenerator), "calendar.lu", @"CognitiveModels\Data")]
        public async Task BatchTestFromLuFile(BatchTestItem batchTestItem)
        {
            _output.WriteLine("Expected:");
            _output.WriteAsFormattedJson(batchTestItem);
            var luisResult = await _luisTester.LuisRecognizer.RecognizeAsync(batchTestItem.Text, CancellationToken.None);

            _output.WriteLine("\r\nActual:");
            _output.WriteAsFormattedJson(luisResult);

            // Assert intent
            Assert.Equal(batchTestItem.Intent, luisResult.GetTopScoringIntent().intent);
            foreach (var entity in batchTestItem.BatchTestEntities)
            {
                var expectedEntityValue = batchTestItem.Text.Substring(entity.StartPos, entity.EndPos - entity.StartPos + 1);
                var actual = luisResult.Entities[entity.Entity].FirstOrDefault()?.ToString();
                Assert.Equal(expectedEntityValue, actual);
            }
        }

        /// <summary>
        /// Class fixture to initialize the LUIS model and the LUIS recognizer
        /// </summary>
        public class LuisTesterFixture : IDisposable
        {
            public LuisTesterFixture()
            {
                // Provision Model
                LuisCommandRunner.LuToLuisJson("blah").Wait();

                // Create LuisRecognizer instance
                var luisApplication = new LuisApplication(
                    Configuration.GetSection("cognitiveModels:calendar:luisAppId").Value,
                    Configuration.GetSection("cognitiveModels:calendar:luisEndpointKey").Value,
                    Configuration.GetSection("cognitiveModels:calendar:luisEndpoint").Value);

                // Create Recognizer instance
                LuisRecognizer = new LuisRecognizer(luisApplication, null, false, null);
            }

            public IConfiguration Configuration => TestConfiguration.Instance.Configuration;

            public LuisRecognizer LuisRecognizer { get; private set; }

            public void Dispose()
            {
                // Tear down model
                // Release recognizer instance
                LuisRecognizer = null;
            }
        }
    }
}
