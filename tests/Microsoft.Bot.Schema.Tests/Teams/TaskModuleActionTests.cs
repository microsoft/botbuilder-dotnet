// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TaskModuleActionTests
    {
        [Theory]
        [InlineData("NullValueButton", null)]
        [InlineData("StringValueButton", "{}")]
        [InlineData("JObjectValueButton", "makeJObject")]
        public void TaskModuleActionInits(string title, object value)
        {
            if ((string)value == "makeJObject")
            {
                value = new JObject();
            }

            var action = new TaskModuleAction(title, value);
            var expectedKey = "type";
            var expectedVal = "task/fetch";

            Assert.NotNull(action);
            Assert.IsType<TaskModuleAction>(action);
            Assert.Equal(title, action.Title);
            var valAsObj = JObject.Parse(action.Value as string);
            Assert.True(valAsObj.ContainsKey(expectedKey));
            Assert.Equal(expectedVal, valAsObj[expectedKey]);
        }
    }
}
