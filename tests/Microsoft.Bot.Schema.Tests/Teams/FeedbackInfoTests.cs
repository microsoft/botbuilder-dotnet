// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class FeedbackInfoTests
    {
        [Fact]
        public void FeedbackInfoInitsWithNoArgs()
        {
            var feedbackLoopInfo = new FeedbackInfo();

            Assert.NotNull(feedbackLoopInfo);
            Assert.IsType<FeedbackInfo>(feedbackLoopInfo);
            Assert.Equal(FeedbackInfoTypes.Default, feedbackLoopInfo.Type);
        }
        
        [Fact]
        public void FeedbackInfoInitsDefault()
        {
            var feedbackLoopInfo = new FeedbackInfo(FeedbackInfoTypes.Default);

            Assert.NotNull(feedbackLoopInfo);
            Assert.IsType<FeedbackInfo>(feedbackLoopInfo);
            Assert.Equal(FeedbackInfoTypes.Default, feedbackLoopInfo.Type);
        }

        [Fact]
        public void FeedbackInfoInitsCustom()
        {
            var feedbackLoopInfo = new FeedbackInfo(FeedbackInfoTypes.Custom);

            Assert.NotNull(feedbackLoopInfo);
            Assert.IsType<FeedbackInfo>(feedbackLoopInfo);
            Assert.Equal(FeedbackInfoTypes.Custom, feedbackLoopInfo.Type);
        }
    }
}
