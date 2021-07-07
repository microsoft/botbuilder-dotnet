// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TaskModuleContinueResponseTests
    {
        [Fact]
        public void TaskModuleContinueResponseInits()
        {
            var value = new TaskModuleTaskInfo();

            var continueResponse = new TaskModuleContinueResponse(value);

            Assert.NotNull(continueResponse);
            Assert.IsType<TaskModuleContinueResponse>(continueResponse);
            Assert.Equal(value, continueResponse.Value);
        }
        
        [Fact]
        public void TaskModuleContinueResponseInitsWithNoArgs()
        {
            var continueResponse = new TaskModuleContinueResponse();

            Assert.NotNull(continueResponse);
            Assert.IsType<TaskModuleContinueResponse>(continueResponse);
        }
    }
}
