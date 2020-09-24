// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    public class HttpHelperTests
    {
        [Fact]
        public async Task WrittenResponseDoesNotIncludeBOM()
        {
            // see https://tools.ietf.org/html/rfc8259#section-8.1
            // Implementations MUST NOT add a byte order mark (U+FEFF) to the
            // beginning of a networked-transmitted JSON text. 

            using (var responseStream = new MemoryStream())
            {
                var responseMock = new Mock<HttpResponse>();
                responseMock.Setup(c => c.Body).Returns(responseStream);

                var invokeResponse = new InvokeResponse() { Status = 200, Body = new[] { "string one", "string two" } };
                await HttpHelper.WriteResponseAsync(responseMock.Object, invokeResponse);

                responseStream.Seek(0, SeekOrigin.Begin);
                var buffer = new byte[4];
                await responseStream.ReadAsync(buffer, 0, 4);

                bool noBomPresent = new UTF8Encoding(true).GetPreamble().Where((p, i) => p != buffer[i]).Any();
                Assert.True(noBomPresent, "HttpHelper.WriteResponseAsync MUST NOT write a BOM to the beginning of the body");
            }
        }
    }
}
