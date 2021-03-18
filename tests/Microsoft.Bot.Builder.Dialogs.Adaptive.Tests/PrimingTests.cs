// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Tests
{
    /// <summary>
    /// Test speech priming functionality.
    /// </summary>
    public class PrimingTests
    {
        public static IEnumerable<object[]> Expected
            => new[]
            {
                new object[] { new AgeEntityRecognizer(), null, new[] { "age" }, null },
                new object[] { new ChannelMentionEntityRecognizer(), null, new[] { "channelMention" }, null },
                new object[] { new ConfirmationEntityRecognizer(), null, new[] { "boolean" }, null },
                new object[] { new CurrencyEntityRecognizer(), null, new[] { "currency" }, null },
                new object[] { new DateTimeEntityRecognizer(), null, new[] { "datetime" }, null },
                new object[] { new DimensionEntityRecognizer(), null, new[] { "dimension" }, null },
                new object[] { new EmailEntityRecognizer(), null, new[] { "email" }, null },
                new object[] { new GuidEntityRecognizer(), null, new[] { "guid" }, null },
                new object[] { new HashtagEntityRecognizer(), null, new[] { "hashtag" }, null },
                new object[] { new IpEntityRecognizer(), null, new[] { "ip" }, null },
                new object[] { new MentionEntityRecognizer(), null, new[] { "mention" }, null },
                new object[] { new NumberEntityRecognizer(), null, new[] { "number" }, null },
                new object[] { new NumberRangeEntityRecognizer(), null, new[] { "numberrange" }, null },
                new object[] { new OrdinalEntityRecognizer(), null, new[] { "ordinal" }, null },
                new object[] { new PercentageEntityRecognizer(), null, new[] { "percentage" }, null },
                new object[] { new PhoneNumberEntityRecognizer(), null, new[] { "phonenumber" }, null },
                new object[] { new RegexEntityRecognizer() { Name = "pattern" }, null, new[] { "pattern" }, null },
                new object[] { new TemperatureEntityRecognizer(), null, new[] { "temperature" }, null },
                new object[] { new UrlEntityRecognizer(), null, new[] { "url" }, null },

                // TODO: chrimc, still need LUIS, QnA and recognizer sets
            };

        [Theory]
        [MemberData(nameof(Expected))]
        public async Task RecognizerDescriptionTests(Recognizer recognizer, string[] intents, string[] entities, string[] lists)
        {
            var description = await recognizer.GetRecognizerDescriptionAsync(GetTurnContext(recognizer));

            if (intents != null)
            {
                Assert.Equal(intents.Length, description.Intents.Count);
                Assert.Collection(description.Intents, (intent) => Assert.Contains(intent.Name, intents));
            }

            if (entities != null)
            {
                Assert.Equal(entities.Length, description.Entities.Count);
                Assert.Collection(description.Entities, (entity) => Assert.Contains(entity.Name, entities));
            }

            if (lists != null)
            {
                Assert.Equal(lists.Length, description.DynamicLists.Count);
                Assert.Collection(description.DynamicLists, (list) => Assert.Contains(list.Entity, lists));
            }
        }

        private DialogContext GetTurnContext(Recognizer recognizer)
        {
            return new DialogContext(
                new DialogSet(),
                new TurnContext(
                    new TestAdapter(TestAdapter.CreateConversation(recognizer.GetType().ToString())),
                    new Schema.Activity()),
                new DialogState());
        }
    }
}
