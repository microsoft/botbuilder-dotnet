// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TaskModuleResponseTests
    {
        [Fact]
        public void TaskModuleResponseInits()
        {
            var task = new TaskModuleResponseBase();
            var cacheInfo = new CacheInfo();

            var response = new TaskModuleResponse(task)
            {
                CacheInfo = cacheInfo
            };

            Assert.NotNull(response);
            Assert.IsType<TaskModuleResponse>(response);
            Assert.Equal(task, response.Task);
            Assert.Equal(cacheInfo, response.CacheInfo);
        }
        
        [Fact]
        public void TaskModuleResponseInitsWithNoArgs()
        {
            var response = new TaskModuleResponse();

            Assert.NotNull(response);
            Assert.IsType<TaskModuleResponse>(response);
        }
    }
}
