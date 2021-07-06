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

        [Fact]
        public void ErrorResponseInits()
        {
            var error = new Error();
            var errorResponse = new ErrorResponse(error);

            Assert.NotNull(errorResponse);
            Assert.IsType<ErrorResponse>(errorResponse);
            Assert.Equal(error, errorResponse.Error);
        }

        [Fact]
        public void ErrorResponseInitsWithNoArgs()
        {
            var errorResponse = new ErrorResponse();

            Assert.NotNull(errorResponse);
            Assert.IsType<ErrorResponse>(errorResponse);
        }

        [Fact]
        public void InnerHttpErrorInits()
        {
            var statusCode = 403;
            var body = new { };

            var innerHttpError = new InnerHttpError(statusCode, body);

            Assert.NotNull(innerHttpError);
            Assert.IsType<InnerHttpError>(innerHttpError);
            Assert.Equal(statusCode, innerHttpError.StatusCode);
            Assert.Equal(body, innerHttpError.Body);
        }

        [Fact]
        public void InnerHttpErrorInitsWithNoArgs()
        {
            var innerHttpError = new InnerHttpError();

            Assert.NotNull(innerHttpError);
            Assert.IsType<InnerHttpError>(innerHttpError);
        }
    }
}
