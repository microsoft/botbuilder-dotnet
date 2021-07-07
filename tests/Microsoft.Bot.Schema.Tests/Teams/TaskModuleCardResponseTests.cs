// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TaskModuleCardResponseTests
    {
        [Fact]
        public void TaskModuleCardResponseInits()
        {
            var value = new TabResponse();
            var cardResponse = new TaskModuleCardResponse(value);

            Assert.NotNull(cardResponse);
            Assert.IsType<TaskModuleCardResponse>(cardResponse);
            Assert.Equal(value, cardResponse.Value);
        }
        
        [Fact]
        public void TaskModuleCardResponseInitsWithNoArgs()
        {
            var cardResponse = new TaskModuleCardResponse();

            Assert.NotNull(cardResponse);
            Assert.IsType<TaskModuleCardResponse>(cardResponse);
        }
    }
}
