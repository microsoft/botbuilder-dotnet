// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Testing.XUnit;
using Microsoft.BotBuilderSamples.Tests.Framework.Luis;
using Microsoft.BotBuilderSamples.Tests.Framework.XUnit;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;
using LuisTestItem = Microsoft.BotBuilderSamples.Tests.Framework.Luis.LuisTestItem;

namespace Microsoft.BotBuilderSamples.Tests.CognitiveModels
{
    // Scenarios
    // intent, entity, utterance, role, version.
    // given a lu model I want to make sure it can be imported, trained and published
    // Xgiven a lu file for a LUIS model ensure that the labeled utterances and entities resolve as expected
    // Xgiven a json file and a LUIS model, I would like to be able to ensure the utterances and entities defined in the json file resolve as expected
    // given a set of utterances, I would like to assert that they map to the expected intent
    // Xgive a deployed model, I want to assert that the intents I need for my bot to work do exists.
    // given a dispatch model I want to assert the child model utterances resolve to my target model through dispatch
    //
    // Assumptions
    // assume local LUIS in container or full model somewhere
    // pure xunit Theories
    // LUDown files as source
    public class CalendarLuisTests : IClassFixture<CalendarLuisTests.LuisTesterFixture>
    {
        private const string SourceLuFile = "calendar.lu";
        private const string RelativePath = @"CognitiveModels/Data";

        private static readonly Lazy<LuisRecognizer> _luisRecognizerLazy = new Lazy<LuisRecognizer>(() =>
        {
            var configuration = TestConfiguration.Instance.Configuration;

            // Create LuisRecognizer instance
            var luisApplication = new LuisApplication(
                configuration.GetSection("cognitiveModels:calendar:luisAppId").Value,
                configuration.GetSection("cognitiveModels:calendar:luisEndpointKey").Value,
                configuration.GetSection("cognitiveModels:calendar:luisEndpoint").Value);

            // Create Recognizer instance
            return new LuisRecognizer(luisApplication, null, false, null);
        });

        private readonly LuisTesterFixture _luisTester;
        private readonly ITestOutputHelper _output;

        public CalendarLuisTests(ITestOutputHelper output, LuisTesterFixture luisTester)
        {
            _luisTester = luisTester;
            _output = output;
        }

        [Theory]
        [LuDownData("calendarTestSet.lu", RelativePath)]
        public async Task CalendarSampleTests(TestDataObject luisData)
        {
            var luisTestItem = luisData.GetObject<LuisTestItem>();

            var luisResult = await _luisRecognizerLazy.Value.RecognizeAsync(luisTestItem.Utterance, CancellationToken.None);
            Assert.Equal(luisTestItem.ExpectedIntent, luisResult.GetTopScoringIntent().intent);
        }

        [Theory]
        [LuDownData(SourceLuFile, RelativePath)]
        public async Task BatchTestFromLuFile(TestDataObject luisData)
        {
            var luisTestItem = luisData.GetObject<LuisTestItem>();

            _output.WriteLine("Expected:");
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

                // foreach (var entityValue in entityValues)
                // {

                // }

                // //Assert.True(resultEntities.ContainsKey(entity.Entity), is);
                // var actual = resultEntities[entity.Entity].FirstOrDefault()?.ToString();
                // Assert.Equal(expectedEntityValue, actual);
            }
        }

        /// <summary>
        /// Class fixture to initialize the LUIS model and the LUIS recognizer.
        /// </summary>
        public class LuisTesterFixture : IDisposable
        {
            public LuisTesterFixture()
            {
                // Provision Model
                LuisCommandRunner.LuToLuisJson(SourceLuFile, Path.Combine(Directory.GetCurrentDirectory(), RelativePath));

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
