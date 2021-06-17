// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class SigninStateVerificationQueryTests
    {
        [Fact]
        public void SigninStateVerificationQueryInits()
        {
            var state = "OK";
            
            var verificationQuery = new SigninStateVerificationQuery(state);

            Assert.NotNull(verificationQuery);
            Assert.IsType<SigninStateVerificationQuery>(verificationQuery);
            Assert.Equal(state, verificationQuery.State);
        }
        
        [Fact]
        public void SigninStateVerificationQueryInitsWithNoArgs()
        {
            var verificationQuery = new SigninStateVerificationQuery();

            Assert.NotNull(verificationQuery);
            Assert.IsType<SigninStateVerificationQuery>(verificationQuery);
        }
    }
}
