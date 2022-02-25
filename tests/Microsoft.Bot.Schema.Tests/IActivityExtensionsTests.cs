// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xunit;
using static Microsoft.Bot.Schema.Tests.ActivityTestData;

namespace Microsoft.Bot.Schema.Tests
{
    public class IActivityExtensionsTests
    {
        [Fact]
        public void SetAndGetLocaleOnConversationUpdate()
        {
            var sut = Activity.CreateConversationUpdateActivity();

            Assert.Null(sut.GetLocale());
            sut.SetLocale("en-UK");
            Assert.Equal("en-UK", sut.GetLocale());
        }

        [Theory]
        [ClassData(typeof(MentionsData))]
        public void DetectMentionedId(List<Entity> entities, bool expectsMention)
        {
            var message = new Activity()
            {
                Type = ActivityTypes.Message,
            };
            ((List<Entity>)message.Entities).AddRange(entities);
            var mentionsId = ActivityExtensions.MentionsId(message, "ChannelAccountId");

            Assert.Equal(expectsMention, mentionsId);
        }

        [Theory]
        [ClassData(typeof(MentionsData))]
        public void DetectsMentionedRecipient(List<Entity> entities, bool expectsMention)
        {
            var recipient = new ChannelAccount
            {
                Id = "ChannelAccountId",
                Name = "ChannelAccountName",
                Role = "ChannelAccountRole",
            };
            recipient.Properties.Add("Name", "Value");

            var message = new Activity()
            {
                Type = ActivityTypes.Message,
                Recipient = recipient,
            };
            ((List<Entity>)message.Entities).AddRange(entities);

            var mentionsRecipient = ActivityExtensions.MentionsRecipient(message);

            Assert.Equal(expectsMention, mentionsRecipient);
        }
    }
}
