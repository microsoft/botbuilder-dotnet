// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TaskModuleMessageResponseTests
    {
        [Fact]
        public void TaskModuleMessageResponseInits()
        {
            var value = "message value for Teams popup";

            var messageResponse = new TaskModuleMessageResponse(value);

            Assert.NotNull(messageResponse);
            Assert.IsType<TaskModuleMessageResponse>(messageResponse);
            Assert.Equal(value, messageResponse.Value);
            Assert.Equal("message", messageResponse.Type);
        }
        
        [Fact]
        public void TaskModuleMessageResponseInitsWithNoArgs()
        {
            var messageResponse = new TaskModuleMessageResponse();

            Assert.NotNull(messageResponse);
            Assert.IsType<TaskModuleMessageResponse>(messageResponse);
            Assert.Equal("message", messageResponse.Type);
        }
    }
}
