// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class ErrorTests
    {
        [Fact]
        public void ErrorInits()
        {
            // no
            var code = "errCode";
            var message = "errMessage";
            var innerHttpError = new InnerHttpError();

            var error = new Error(code, message, innerHttpError);

            Assert.NotNull(error);
            Assert.IsType<Error>(error);
            Assert.Equal(code, error.Code);
            Assert.Equal(message, error.Message);
            Assert.Equal(innerHttpError, error.InnerHttpError);
        }

        [Fact]
        public void ErrorInitsWithNoArgs()
        {
            var error = new Error();

            Assert.NotNull(error);
            Assert.IsType<Error>(error);
        }
    }
}
