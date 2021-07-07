// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TaskModuleTaskInfoTests
    {
        [Fact]
        public void TaskModuleTaskInfoInits()
        {
            var title = "chatty";
            var height = "medium";
            var width = "large";
            var url = "https://example.com";
            var card = new Attachment();
            var fallbackUrl = "https://fallback-url-of-example.com";
            var completionBotId = "0000-0000-0000-0000-0000";

            var taskInfo = new TaskModuleTaskInfo(title, height, width, url, card, fallbackUrl, completionBotId);

            Assert.NotNull(taskInfo);
            Assert.IsType<TaskModuleTaskInfo>(taskInfo);
            Assert.Equal(title, taskInfo.Title);
            Assert.Equal(height, taskInfo.Height);
            Assert.Equal(width, taskInfo.Width);
            Assert.Equal(url, taskInfo.Url);
            Assert.Equal(card, taskInfo.Card);
            Assert.Equal(fallbackUrl, taskInfo.FallbackUrl);
            Assert.Equal(completionBotId, taskInfo.CompletionBotId);
        }
        
        [Fact]
        public void TaskModuleTaskInfoInitsWithNoArgs()
        {
            var taskInfo = new TaskModuleTaskInfo();

            Assert.NotNull(taskInfo);
            Assert.IsType<TaskModuleTaskInfo>(taskInfo);
        }
    }
}
