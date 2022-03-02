// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Bot.Connector.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Connector.Schema.Tests.Teams
{
    public class TaskModuleActionTests
    {
        [Theory]
        [InlineData("NullValueButton", null)]
        [InlineData("StringValueButton", "{}")]
        [InlineData("ObjectValueButton", "makeObject")]
        public void ConstructorTests(string title, object value)
        {
            if ((string)value == "makeObject")
            {
                value = new Dictionary<string, JsonElement>();
            }

            var action = new TaskModuleAction(title, value);
            var expectedKey = "type";
            var expectedVal = "task/fetch";
            var valAsObj = action.Value.ToJsonElements();

            Assert.NotNull(action);
            Assert.IsType<TaskModuleAction>(action);
            Assert.Equal(title, action.Title);
            Assert.True(valAsObj.ContainsKey(expectedKey));
            Assert.Equal(expectedVal, valAsObj[expectedKey].GetString());
        }
    }
}
