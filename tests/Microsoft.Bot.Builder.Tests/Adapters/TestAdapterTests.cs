// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests.Adapters
{
    [TestClass]
    public class TestAdapterTests
    {
        [DataTestMethod]
        [DataRow(Channels.Test)]
        [DataRow(Channels.Emulator)]
        [DataRow(Channels.Msteams)]
        [DataRow(Channels.Webchat)]
        [DataRow(Channels.Cortana)]
        [DataRow(Channels.Directline)]
        [DataRow(Channels.Facebook)]
        [DataRow(Channels.Slack)]
        [DataRow(Channels.Telegram)]
        public async Task ShouldUseCustomChannelId(string targetChannel)
        {
            var sut = new TestAdapter(targetChannel);

            var receivedChannelId = string.Empty;
            async Task TestCallback(ITurnContext context, CancellationToken token)
            {
                receivedChannelId = context.Activity.ChannelId;
                await context.SendActivityAsync("reply from the bot", cancellationToken: token);
            }

            await sut.SendTextToBotAsync("test", TestCallback, CancellationToken.None);
            var reply = sut.GetNextReply();
            Assert.AreEqual(targetChannel, receivedChannelId);
            Assert.AreEqual(targetChannel, reply.ChannelId);
        }
    }
}
