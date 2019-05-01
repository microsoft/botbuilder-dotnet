// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.Tests.Extensions
{
    public static class LuisRecognizerEx
    {
        /// <summary>
        /// Extension method that takes a string for the utterance rather than a TurnContext.
        /// </summary>
        public static async Task<RecognizerResult> RecognizeAsync(this LuisRecognizer recognizer, string utterance, CancellationToken cancellationToken)
        {
            var b = new TestAdapter();
            var a = new Activity
            {
                Type = ActivityTypes.Message,
                Text = utterance,
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount(),
            };
            var tc = new TurnContext(b, a);
            return await recognizer.RecognizeAsync<RecognizerResult>(tc, cancellationToken);
        }
    }
}
