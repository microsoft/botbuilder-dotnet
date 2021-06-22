// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class ExpectedRepliesTests
    {
        [Fact]
        public void ExpectedRepliesInits()
        {
            var activities = new List<Activity>() { new Activity() };
            
            var expectedReplies = new ExpectedReplies(activities);

            Assert.NotNull(expectedReplies);
            Assert.IsType<ExpectedReplies>(expectedReplies);
            Assert.Equal(activities, expectedReplies.Activities);
        }

        [Fact]
        public void ExpectedRepliesInitsWithNoArgs()
        {
            var expectedReplies = new ExpectedReplies();

            Assert.NotNull(expectedReplies);
            Assert.IsType<ExpectedReplies>(expectedReplies);
        }
    }
}
