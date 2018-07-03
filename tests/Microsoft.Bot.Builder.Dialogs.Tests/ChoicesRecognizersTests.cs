// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Choice Tests")]
    public class ChoicesRecognizersTests
    {
        private static List<SortedValue> colorValues = new List<SortedValue> {
            new SortedValue { Value = "red", Index = 0 },
            new SortedValue { Value = "green", Index = 1 },
            new SortedValue { Value = "blue", Index = 2 }
        };

        private static List<SortedValue> overlappingValues = new List<SortedValue> {
            new SortedValue { Value = "bread", Index = 0 },
            new SortedValue { Value = "bread pudding", Index = 1 },
            new SortedValue { Value = "pudding", Index = 2 }
        };

        private static List<SortedValue> similarValues = new List<SortedValue> {
            new SortedValue { Value = "option A", Index = 0 },
            new SortedValue { Value = "option B", Index = 1 },
            new SortedValue { Value = "option C", Index = 2 }
        };

        // FindValues
    
        [TestMethod]
        public void ShouldFindASimpleValueInAnSingleWordUtterance()
        {
            var found = Find.FindValues("red", colorValues);
            Assert.AreEqual(1, found.Count);
            AssertResult(found[0], 0, 2, "red");
            AssertValue(found[0], "red", 0, 1.0f);
        }

        [TestMethod]
        public void ShouldFindASimpleValueInAnUtterance()
        {
            var found = Find.FindValues("the red one please.", colorValues);
            Assert.AreEqual(1, found.Count);
            AssertResult(found[0], 4, 6, "red");
            AssertValue(found[0], "red", 0, 1.0f);
        }

        [TestMethod]
        public void ShouldFindMultipleValuesWithinAnUtterance()
        {
            var found = Find.FindValues("the red and blue ones please.", colorValues);
            Assert.AreEqual(2, found.Count);
            AssertResult(found[0], 4, 6, "red");
            AssertValue(found[0], "red", 0, 1.0f);
            AssertValue(found[1], "blue", 2, 1.0f);
        }

        [TestMethod]
        public void ShouldFindMultipleValuesThatOverlap()
        {
            var found = Find.FindValues("the bread pudding and bread please.", overlappingValues);
            Assert.AreEqual(2, found.Count);
            AssertResult(found[0], 4, 16, "bread pudding");
            AssertValue(found[0], "bread pudding", 1, 1.0f);
            AssertValue(found[1], "bread", 0, 1.0f);
        }

        [TestMethod]
        public void ShouldCorrectlyDisambiguateBetweenVerySimilarValues()
        {
            var found = Find.FindValues("option B", similarValues, new FindValuesOptions { AllowPartialMatches = true });
            Assert.AreEqual(1, found.Count);
            AssertValue(found[0], "option B", 1, 1.0f);
        }

        // FindChoices

        private static List<string> colorChoices = new List<string> { "red", "green", "blue" };
        private static List<string> overlappingChoices = new List<string> { "bread", "bread pudding", "pudding" };

        [TestMethod]
        public void ShouldFindASingleChoiceInAnUtterance()
        {
            var found = Find.FindChoices("the red one please.", colorChoices);
            Assert.AreEqual(1, found.Count);
            AssertResult(found[0], 4, 6, "red");
            AssertChoice(found[0], "red", 0, 1.0f, "red");
        }

        [TestMethod]
        public void ShouldFindMultipleChoicesWithinAnUtterance()
        {
            var found = Find.FindChoices("the red and blue ones please.", colorChoices);
            Assert.AreEqual(2, found.Count);
            AssertResult(found[0], 4, 6, "red");
            AssertChoice(found[0], "red", 0, 1.0f);
            AssertChoice(found[1], "blue", 2, 1.0f);
        }

        [TestMethod]
        public void ShouldFindMultipleChoicesThatOverlap()
        {
            var found = Find.FindChoices("the bread pudding and bread please.", overlappingChoices);
            Assert.AreEqual(2, found.Count);
            AssertResult(found[0], 4, 16, "bread pudding");
            AssertChoice(found[0], "bread pudding", 1, 1.0f);
            AssertChoice(found[1], "bread", 0, 1.0f);
        }

        // RecognizeChoice

        [TestMethod]
        public void ShouldFindAChoiceInAnUtteranceByName()
        {
            var found = ChoiceRecognizers.RecognizeChoices("the red one please.", colorChoices);
            Assert.AreEqual(1, found.Count);
            AssertResult(found[0], 4, 6, "red");
            AssertChoice(found[0], "red", 0, 1.0f, "red");
        }

        [TestMethod]
        public void ShouldFindAChoiceInAnUtteranceByOrdinalPosition()
        {
            var found = ChoiceRecognizers.RecognizeChoices("the first one please.", colorChoices);
            Assert.AreEqual(1, found.Count);
            AssertResult(found[0], 4, 8, "first");
            AssertChoice(found[0], "red", 0, 1.0f);
        }

        [TestMethod]
        public void ShouldFindMultipleChoicesInAnUtteranceByOrdinalPosition()
        {
            var found = ChoiceRecognizers.RecognizeChoices("the first and third one please.", colorChoices);
            Assert.AreEqual(2, found.Count);
            AssertChoice(found[0], "red", 0, 1.0f);
            AssertChoice(found[1], "blue", 2, 1.0f);
        }

        [TestMethod]
        public void ShouldFindAChoiceInAnUtteranceByNumericalIndex_digit()
        {
            var found = ChoiceRecognizers.RecognizeChoices("1", colorChoices);
            Assert.AreEqual(1, found.Count);
            AssertResult(found[0], 0, 0, "1");
            AssertChoice(found[0], "red", 0, 1.0f);
        }

        [TestMethod]
        public void ShouldFindAChoiceInAnUtteranceByNumericalIndex_Text()
        {
            var found = ChoiceRecognizers.RecognizeChoices("one", colorChoices);
            Assert.AreEqual(1, found.Count);
            AssertResult(found[0], 0, 2, "one");
            AssertChoice(found[0], "red", 0, 1.0f);
        }

        [TestMethod]
        public void ShouldFindMultipleChoicesInAnUtteranceByNumerical_index()
        {
            var found = ChoiceRecognizers.RecognizeChoices("option one and 3.", colorChoices);
            Assert.AreEqual(2, found.Count);
            AssertChoice(found[0], "red", 0, 1.0f);
            AssertChoice(found[1], "blue", 2, 1.0f);
        }

        // Helper functions

        private static void AssertResult<T>(ModelResult<T> result, int start, int end, string text)
        {
            Assert.AreEqual(start, result.Start);
            Assert.AreEqual(end, result.End);
            Assert.AreEqual(text, result.Text);
        }

        private static void AssertValue(ModelResult<FoundValue> result, string value, int index, float score)
        {
            Assert.AreEqual("value", result.TypeName);
            Assert.IsNotNull(result.Resolution);
            var resolution = result.Resolution;
            Assert.AreEqual(value, resolution.Value);
            Assert.AreEqual(index, resolution.Index);
            Assert.AreEqual(score, resolution.Score);
        }

        private static void AssertChoice(ModelResult<FoundChoice> result, string value, int index, float score, string synonym = null)
        {
            Assert.AreEqual("choice", result.TypeName);
            Assert.IsNotNull(result.Resolution);
            var resolution = result.Resolution;
            Assert.AreEqual(value, resolution.Value);
            Assert.AreEqual(index, resolution.Index);
            Assert.AreEqual(score, resolution.Score);
            if (synonym != null)
            {
                Assert.AreEqual(synonym, resolution.Synonym);
            }
        }
    }
}
