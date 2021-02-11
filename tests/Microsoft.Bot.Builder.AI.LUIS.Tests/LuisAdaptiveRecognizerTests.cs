// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using AdaptiveExpressions.Converters;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.AI.Luis;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    public class LuisAdaptiveRecognizerTests : IClassFixture<LuisAdaptiveRecognizerFixture>
    {
        private const string DynamicListJSon = @"[
                {
                    'entity': 'alphaEntity',
                    'list': [
                        {
                            'canonicalForm': 'a',
                            'synonyms': [
                                'a',
                                'aa'
                            ]
                        },
                        {
                            'canonicalForm': 'b',
                            'synonyms': [
                                'b',
                                'bb'
                            ]
}
                    ]
                },
                {
                    'entity': 'numberEntity',
                    'list': [
                        {
                            'canonicalForm': '1',
                            'synonyms': [
                                '1',
                                'one'
                            ]
                        },
                        {
                            'canonicalForm': '2',
                            'synonyms': [
                                '2',
                                'two'
                            ]
                        }
                    ]
                }
            ]";

        private const string RecognizerJson = @"{
            '$kind': 'Microsoft.LuisRecognizer',
            'applicationId': '=settings.luis.DynamicLists_test_en_us_lu.appId',
            'endpoint': '=settings.luis.endpoint',
            'endpointKey': '=settings.luis.endpointKey', 'dynamicLists': " + DynamicListJSon + "}";

        private readonly LuisAdaptiveRecognizerFixture _luisAdaptiveRecognizerFixture;

        public LuisAdaptiveRecognizerTests(LuisAdaptiveRecognizerFixture luisAdaptiveRecognizerFixture)
        {
            _luisAdaptiveRecognizerFixture = luisAdaptiveRecognizerFixture;
        }

        [Fact]
        public async Task DynamicLists()
        {
            await TestUtils.RunTestScript(_luisAdaptiveRecognizerFixture.ResourceExplorer, configuration: _luisAdaptiveRecognizerFixture.Configuration);
        }

        [Fact]
        public async Task DynamicListsExpression()
        {
            await TestUtils.RunTestScript(_luisAdaptiveRecognizerFixture.ResourceExplorer, configuration: _luisAdaptiveRecognizerFixture.Configuration);
        }

        [Fact]
        public async Task ExternalEntities()
        {
            await TestUtils.RunTestScript(_luisAdaptiveRecognizerFixture.ResourceExplorer, configuration: _luisAdaptiveRecognizerFixture.Configuration);
        }

        [Fact]
        public void DeserializeDynamicList()
        {
            var dl = JsonConvert.DeserializeObject<List<DynamicList>>(DynamicListJSon);
            Assert.Equal(2, dl.Count);
            Assert.Equal("alphaEntity", dl[0].Entity);
            Assert.Equal(2, dl[0].List.Count);
        }

        [Fact]
        public void DeserializeSerializedDynamicList()
        {
            var ol = JsonConvert.DeserializeObject<List<DynamicList>>(DynamicListJSon);
            var json = JsonConvert.SerializeObject(ol);
            var dl = JsonConvert.DeserializeObject<List<DynamicList>>(json);
            Assert.Equal(2, dl.Count);
            Assert.Equal("alphaEntity", dl[0].Entity);
            Assert.Equal(2, dl[0].List.Count);
        }

        [Fact]
        public void DeserializeArrayExpression()
        {
            var ae = JsonConvert.DeserializeObject<ArrayExpression<DynamicList>>(DynamicListJSon, new ArrayExpressionConverter<DynamicList>());
            var dl = ae.GetValue(null);
            Assert.Equal(2, dl.Count);
            Assert.Equal("alphaEntity", dl[0].Entity);
            Assert.Equal(2, dl[0].List.Count);
        }

        [Fact]
        public void DeserializeLuisAdaptiveRecognizer()
        {
            var recognizer = JsonConvert.DeserializeObject<LuisAdaptiveRecognizer>(RecognizerJson, new ArrayExpressionConverter<DynamicList>());
            var dl = recognizer.DynamicLists.GetValue(null);
            Assert.Equal(2, dl.Count);
            Assert.Equal("alphaEntity", dl[0].Entity);
            Assert.Equal(2, dl[0].List.Count);
        }
    }
}
