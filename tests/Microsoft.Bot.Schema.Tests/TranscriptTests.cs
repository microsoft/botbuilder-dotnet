// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class TranscriptTests
    {
        [Fact]
        public void TranscriptInits()
        {
            var activities = new List<Activity>();

            var transcript = new Transcript(activities);

            Assert.NotNull(transcript);
            Assert.IsType<Transcript>(transcript);
            Assert.Equal(activities, transcript.Activities);
        }
        
        [Fact]
        public void TranscriptInitsWithNoArgs()
        {
            var transcript = new Transcript();

            Assert.NotNull(transcript);
            Assert.IsType<Transcript>(transcript);
        }
    }
}
