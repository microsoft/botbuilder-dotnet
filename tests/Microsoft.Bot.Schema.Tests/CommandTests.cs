// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class CommandTests
    {
        [Fact]
        public void CommandResultValueInits()
        {
            var id = "myCommandId";
            var data = new { };
            var err = new Error("500", "test error");
            var commandResultValue = new CommandResultValue<object>()
            {
                CommandId = id,
                Data = data,
                Error = err
            };

            Assert.NotNull(commandResultValue);
            Assert.IsType<CommandResultValue<object>>(commandResultValue);
            Assert.Equal(id, commandResultValue.CommandId);
            Assert.Equal(data, commandResultValue.Data);
            Assert.Equal(err, commandResultValue.Error);
        }

        [Fact]
        public void CommandValueInits()
        {
            var id = "myCommandId";
            var data = new { };
            var commandValue = new CommandValue<object>()
            {
                CommandId = id,
                Data = data,
            };

            Assert.NotNull(commandValue);
            Assert.IsType<CommandValue<object>>(commandValue);
            Assert.Equal(id, commandValue.CommandId);
            Assert.Equal(data, commandValue.Data);
        }
    }
}
