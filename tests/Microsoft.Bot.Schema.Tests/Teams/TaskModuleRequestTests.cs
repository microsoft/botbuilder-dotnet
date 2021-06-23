// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TaskModuleRequestTests
    {
        [Fact]
        public void TaskModuleRequestInits()
        {
            var data = new JObject() { { "key", "value" } };
            var context = new TaskModuleRequestContext();
            var tabEntityContext = new TabEntityContext();

            var request = new TaskModuleRequest(data, context)
            {
                TabEntityContext = tabEntityContext
            };

            Assert.NotNull(request);
            Assert.IsType<TaskModuleRequest>(request);
            Assert.Equal(data, request.Data);
            Assert.Equal(context, request.Context);
            Assert.Equal(tabEntityContext, request.TabEntityContext);
        }
        
        [Fact]
        public void TaskModuleRequestInitsWithNoArgs()
        {
            var request = new TaskModuleRequest();

            Assert.NotNull(request);
            Assert.IsType<TaskModuleRequest>(request);
        }
    }
}
