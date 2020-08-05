// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class InvokeResponseOfTTests
    {
        [Fact]
        public void BodyDoesntHideBase()
        {
            var sut = new InvokeResponse<SomeType>
            {
                Status = 200,
                Body = new SomeType
                {
                    Id = "200",
                    Value = "blah"
                }
            };

            // Assert that the Body property is not hidden when using InvokeResponse or InvokeResponse<T>.
            var sutAsInvokeResponse = (InvokeResponse)sut;
            Assert.NotNull(sutAsInvokeResponse.Body);
            Assert.Same(sut.Body, sutAsInvokeResponse.Body);
            Assert.Equal(sut.Body.Id, ((SomeType)sutAsInvokeResponse.Body).Id);
            Assert.Equal(sut.Body.Value, ((SomeType)sutAsInvokeResponse.Body).Value);
        }

        private class SomeType
        {
            public string Id { get; set; }

            public string Value { get; set; }
        }
    }
}
