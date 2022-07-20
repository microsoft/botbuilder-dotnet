// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class ChannelInfoTests
    {
        [Fact]
        public void ChannelInfoInits()
        {
            var id = "channelId";
            var name = "watercooler";
            var type = "standard";

            var channelInfo = new ChannelInfo(id, name);
            channelInfo.Type = type;

            Assert.NotNull(channelInfo);
            Assert.IsType<ChannelInfo>(channelInfo);
            Assert.Equal(id, channelInfo.Id);
            Assert.Equal(name, channelInfo.Name);
            Assert.Equal(type, channelInfo.Type);
        }

        [Fact]
        public void ChannelInfoInitsWithNoArgs()
        {
            var channelInfo = new ChannelInfo();

            Assert.NotNull(channelInfo);
            Assert.IsType<ChannelInfo>(channelInfo);
        }
    }
}
