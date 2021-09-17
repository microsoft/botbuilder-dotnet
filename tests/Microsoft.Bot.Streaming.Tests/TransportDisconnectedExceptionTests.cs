// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.PayloadTransport;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests
{
    public class TransportDisconnectedExceptionTests
    {
        [Fact]

        public void TransportDisconnectedException_ctor()
        {
            var exception = new TransportDisconnectedException();

            Assert.NotNull(exception.Reason);
        }

        [Fact]

        public void TransportDisconnectedException_ctor_With_InnerException()
        {
            var innerException = new Exception("inner-exception");
            var exception = new TransportDisconnectedException("exception", innerException);

            Assert.Equal(innerException, exception.InnerException);
            Assert.Equal("exception", exception.Reason);
        }
    }
}
