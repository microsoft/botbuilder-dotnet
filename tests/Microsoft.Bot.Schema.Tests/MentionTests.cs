// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class MentionTests
    {
        [Fact]
        public void MentionInits()
        {
            var mentioned = new ChannelAccount("Id1", "Name1", "Role1", "aadObjectId1");
            var text = "hi @Name1";
            var type = "mention";

            var mention = new Mention(mentioned, text, type);

            Assert.NotNull(mention);
            Assert.IsType<Mention>(mention);
            Assert.Equal(mentioned, mention.Mentioned);
            Assert.Equal(text, mention.Text);
            Assert.Equal(type, mention.Type);
        }

        [Fact]
        public void MentionInitsWithNoArgs()
        {
            var mention = new Mention();

            Assert.NotNull(mention);
            Assert.IsType<Mention>(mention);
        }
    }
}
