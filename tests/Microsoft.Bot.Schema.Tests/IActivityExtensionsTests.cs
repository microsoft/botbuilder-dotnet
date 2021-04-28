// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

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

        [Fact]
        public void DetectIdMentionInText()
        {
            var message = new Activity()
            {
                Type = ActivityTypes.Message,
                Entities = new List<Entity>() { new Entity() },
            };
            var mentionsId = ActivityExtensions.MentionsId(message, "myId");

            Assert.False(mentionsId);
        }
    }
}
