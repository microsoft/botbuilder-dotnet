// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Payloads;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests
{
    public class CancelDisassemblerTests
    {
        [Fact]
        public async Task CancelDisassembler_Disassembles()
        {
            var sender = new MockPayloadSender();
            var id = Guid.NewGuid();
            const char type = 'X';

            var disassembler = new CancelDisassembler(sender, id, type);

            await disassembler.Disassemble();

            var header = sender.SentHeaders.First();
            Assert.Equal(id, header.Id);
        }
    }
}
