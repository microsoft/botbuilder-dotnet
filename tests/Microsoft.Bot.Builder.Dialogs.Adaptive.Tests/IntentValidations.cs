// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    /// <summary>
    /// Validations to run for specific intents specified in <see cref="RecognizerTelemetryUtils"/>.
    /// </summary>
    internal static class IntentValidations
    {
        /// <summary>
        /// Validates the codeIntent utterance "intent a1 b2".
        /// </summary>
        /// <param name="result">The <see cref="RecognizerResult"/>.</param>
        internal static void ValidateCodeIntent(RecognizerResult result)
        {
            // intent assertions
            Assert.Single(result.Intents);
            Assert.Equal("codeIntent", result.Intents.Select(i => i.Key).First());

            // entity assertions from capture group
            dynamic entities = result.Entities;
            Assert.NotNull(entities.code);
            Assert.Null(entities.color);
            Assert.Equal(2, entities.code.Count);
            Assert.Equal("a1", (string)entities.code[0]);
            Assert.Equal("b2", (string)entities.code[1]);
        }

        /// <summary>
        /// Validates the colorIntent utterance "I would like colors red and orange".
        /// </summary>
        /// <param name="result">The <see cref="RecognizerResult"/>.</param>
        internal static void ValidateColorIntent(RecognizerResult result)
        {
            Assert.Single(result.Intents);
            Assert.Equal("colorIntent", result.Intents.Select(i => i.Key).First());

            // entity assertions from capture group
            dynamic entities = result.Entities;
            Assert.NotNull(entities.color);
            Assert.Null(entities.code);
            Assert.Equal(2, entities.color.Count);
            Assert.Equal("red", (string)entities.color[0]);
            Assert.Equal("orange", (string)entities.color[1]);
        }

        internal static void ValidateGreetingIntent(RecognizerResult result)
        {
            Assert.Single(result.Intents);
            Assert.Equal("Greeting", result.Intents.Select(i => i.Key).First());
        }

        internal static void ValidateChooseIntent(RecognizerResult result)
        {
            Assert.Single(result.Intents);
            Assert.Equal("ChooseIntent", result.Intents.Select(i => i.Key).First());
        }

        internal static void ValidateXIntent(RecognizerResult result)
        {
            Assert.Single(result.Intents);
            Assert.Equal("x", result.Intents.Select(i => i.Key).First());
        }
    }
}
