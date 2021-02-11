// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [Trait("TestCategory", "Prompts")]
    [Trait("TestCategory", "Choice Tests")]
    public class ChoicesRecognizersTests
    {
        // FindChoices
        private static List<string> colorChoices = new List<string> { "red", "green", "blue" };
        private static List<string> overlappingChoices = new List<string> { "bread", "bread pudding", "pudding" };

        private static List<SortedValue> colorValues = new List<SortedValue>
        {
            new SortedValue { Value = "red", Index = 0 },
            new SortedValue { Value = "green", Index = 1 },
            new SortedValue { Value = "blue", Index = 2 },
        };

        private static List<SortedValue> overlappingValues = new List<SortedValue>
        {
            new SortedValue { Value = "bread", Index = 0 },
            new SortedValue { Value = "bread pudding", Index = 1 },
            new SortedValue { Value = "pudding", Index = 2 },
        };

        private static List<SortedValue> similarValues = new List<SortedValue>
        {
            new SortedValue { Value = "option A", Index = 0 },
            new SortedValue { Value = "option B", Index = 1 },
            new SortedValue { Value = "option C", Index = 2 },
        };

        private static List<SortedValue> valuesWithSpecialCharacters = new List<SortedValue>
        {
            new SortedValue { Value = "A < B", Index = 0 },
            new SortedValue { Value = "A >= B", Index = 1 },
            new SortedValue { Value = "A ??? B", Index = 2 },
        };

        // FindValues
        [Fact]
        public void ShouldFindASimpleValueInAnSingleWordUtterance()
        {
            var found = Find.FindValues("red", colorValues);
            Assert.Single(found);
            AssertResult(found[0], 0, 2, "red");
            AssertValue(found[0], "red", 0, 1.0f);
        }

        [Fact]
        public void ShouldFindASimpleValueInAnUtterance()
        {
            var found = Find.FindValues("the red one please.", colorValues);
            Assert.Single(found);
            AssertResult(found[0], 4, 6, "red");
            AssertValue(found[0], "red", 0, 1.0f);
        }

        [Fact]
        public void ShouldFindMultipleValuesWithinAnUtterance()
        {
            var found = Find.FindValues("the red and blue ones please.", colorValues);
            Assert.Equal(2, found.Count);
            AssertResult(found[0], 4, 6, "red");
            AssertValue(found[0], "red", 0, 1.0f);
            AssertValue(found[1], "blue", 2, 1.0f);
        }

        [Fact]
        public void ShouldFindMultipleValuesThatOverlap()
        {
            var found = Find.FindValues("the bread pudding and bread please.", overlappingValues);
            Assert.Equal(2, found.Count);
            AssertResult(found[0], 4, 16, "bread pudding");
            AssertValue(found[0], "bread pudding", 1, 1.0f);
            AssertValue(found[1], "bread", 0, 1.0f);
        }

        [Fact]
        public void ShouldCorrectlyDisambiguateBetweenVerySimilarValues()
        {
            var found = Find.FindValues("option B", similarValues, new FindValuesOptions { AllowPartialMatches = true });
            Assert.Single(found);
            AssertValue(found[0], "option B", 1, 1.0f);
        }

        [Fact]
        public void ShouldPreferExactMatch()
        {
            var index = 1;
            var utterance = valuesWithSpecialCharacters[index].Value;
            var found = Find.FindValues(utterance, valuesWithSpecialCharacters);

            AssertValue(found.Single(), utterance, index, 1);
        }

        [Fact]
        public void ShouldFindASingleChoiceInAnUtterance()
        {
            var found = Find.FindChoices("the red one please.", colorChoices);
            Assert.Single(found);
            AssertResult(found[0], 4, 6, "red");
            AssertChoice(found[0], "red", 0, 1.0f, "red");
        }

        [Fact]
        public void ShouldFindMultipleChoicesWithinAnUtterance()
        {
            var found = Find.FindChoices("the red and blue ones please.", colorChoices);
            Assert.Equal(2, found.Count);
            AssertResult(found[0], 4, 6, "red");
            AssertChoice(found[0], "red", 0, 1.0f);
            AssertChoice(found[1], "blue", 2, 1.0f);
        }

        [Fact]
        public void ShouldFindMultipleChoicesThatOverlap()
        {
            var found = Find.FindChoices("the bread pudding and bread please.", overlappingChoices);
            Assert.Equal(2, found.Count);
            AssertResult(found[0], 4, 16, "bread pudding");
            AssertChoice(found[0], "bread pudding", 1, 1.0f);
            AssertChoice(found[1], "bread", 0, 1.0f);
        }

        [Fact]
        public void ShouldAcceptNullUtteranceInFindChoices()
        {
            var found = Find.FindChoices(null, colorChoices);
            Assert.Empty(found);
        }

        // RecognizeChoices
        [Fact]
        public void ShouldFindAChoiceInAnUtteranceByName()
        {
            var found = ChoiceRecognizers.RecognizeChoices("the red one please.", colorChoices);
            Assert.Single(found);
            AssertResult(found[0], 4, 6, "red");
            AssertChoice(found[0], "red", 0, 1.0f, "red");
        }

        [Fact]
        public void ShouldFindAChoiceInAnUtteranceByOrdinalPosition()
        {
            var found = ChoiceRecognizers.RecognizeChoices("the first one please.", colorChoices);
            Assert.Single(found);
            AssertResult(found[0], 4, 8, "first");
            AssertChoice(found[0], "red", 0, 1.0f);
        }

        [Fact]
        public void ShouldFindMultipleChoicesInAnUtteranceByOrdinalPosition()
        {
            var found = ChoiceRecognizers.RecognizeChoices("the first and third one please.", colorChoices);
            Assert.Equal(2, found.Count);
            AssertChoice(found[0], "red", 0, 1.0f);
            AssertChoice(found[1], "blue", 2, 1.0f);
        }

        [Fact]
        public void ShouldFindAChoiceInAnUtteranceByNumericalIndex_digit()
        {
            var found = ChoiceRecognizers.RecognizeChoices("1", colorChoices);
            Assert.Single(found);
            AssertResult(found[0], 0, 0, "1");
            AssertChoice(found[0], "red", 0, 1.0f);
        }

        [Fact]
        public void ShouldFindAChoiceInAnUtteranceByNumericalIndex_Text()
        {
            var found = ChoiceRecognizers.RecognizeChoices("one", colorChoices);
            Assert.Single(found);
            AssertResult(found[0], 0, 2, "one");
            AssertChoice(found[0], "red", 0, 1.0f);
        }

        [Fact]
        public void ShouldFindMultipleChoicesInAnUtteranceByNumerical_index()
        {
            var found = ChoiceRecognizers.RecognizeChoices("option one and 3.", colorChoices);
            Assert.Equal(2, found.Count);
            AssertChoice(found[0], "red", 0, 1.0f);
            AssertChoice(found[1], "blue", 2, 1.0f);
        }

        [Fact]
        public void ShouldAcceptNullUtteranceInRecognizeChoices()
        {
            var found = ChoiceRecognizers.RecognizeChoices(null, colorChoices);
            Assert.Empty(found);
        }

        [Fact]
        public void ShouldNOTFindAChoiceInAnUtteranceByOrdinalPosition_RecognizeOrdinalsFalseAndRecognizeNumbersFalse()
        {
            var found = ChoiceRecognizers.RecognizeChoices("the first one please.", colorChoices, new FindChoicesOptions() { RecognizeOrdinals = false, RecognizeNumbers = false });
            Assert.Empty(found);
        }

        [Fact]
        public void ShouldNOTFindAChoiceInAnUtteranceByNumericalIndex_Text_RecognizeNumbersFalse()
        {
            var found = ChoiceRecognizers.RecognizeChoices("one", colorChoices, new FindChoicesOptions() { RecognizeNumbers = false });
            Assert.Empty(found);
        }

        // Helper functions
        private static void AssertResult<T>(ModelResult<T> result, int start, int end, string text)
        {
            Assert.Equal(start, result.Start);
            Assert.Equal(end, result.End);
            Assert.Equal(text, result.Text);
        }

        private static void AssertValue(ModelResult<FoundValue> result, string value, int index, float score)
        {
            Assert.Equal("value", result.TypeName);
            Assert.NotNull(result.Resolution);
            var resolution = result.Resolution;
            Assert.Equal(value, resolution.Value);
            Assert.Equal(index, resolution.Index);
            Assert.Equal(score, resolution.Score);
        }

        private static void AssertChoice(ModelResult<FoundChoice> result, string value, int index, float score, string synonym = null)
        {
            Assert.Equal("choice", result.TypeName);
            Assert.NotNull(result.Resolution);
            var resolution = result.Resolution;
            Assert.Equal(value, resolution.Value);
            Assert.Equal(index, resolution.Index);
            Assert.Equal(score, resolution.Score);
            if (synonym != null)
            {
                Assert.Equal(synonym, resolution.Synonym);
            }
        }
    }
}
