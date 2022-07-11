// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class ReadReceiptInfoTests
    {
        [Theory]
        [InlineData("1000", "1000", true)]
        [InlineData("1001", "1000", true)]
        [InlineData("1000", "1001", false)]
        [InlineData("1000", null, false)]
        [InlineData(null, "1000", false)]
        public void ReadReceiptInfoTest(string lastRead, string compare, bool isRead)
        {
            var info = new ReadReceiptInfo(lastRead);

            Assert.Equal(info.LastReadMessageId, lastRead);
            Assert.Equal(info.IsMessageRead(compare), isRead);
            Assert.Equal(ReadReceiptInfo.IsMessageRead(compare, lastRead), isRead);
        }
    }
}
