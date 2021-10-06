// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging.Events;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests.Events
{
    public sealed class EventsTests
    {
        [Fact]
        public void Events_Reset()
        {
            IEvents events = new Events<DialogEvents>();

            Assert.True(events["activityReceived"]);
            Assert.True(events["error"]);

            events["testFilter"] = true;

            var filters = new List<string>
            {
                "error", "testFilter"
            };

            events.Reset(filters);

            Assert.False(events["activityReceived"]);
            Assert.True(events["error"]);
            Assert.True(events["testFilter"]);
        }
    }
}
