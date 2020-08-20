// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks
{
    /// <summary>
    /// Convert and store the actual data of HttpResonseMock.
    /// </summary>
    public class HttpResponseMockMessage
    {
        private readonly HttpResponseMock mock;
        private readonly HttpResponseMockContent content;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseMockMessage"/> class.
        /// Contain OK(200) status code, an empty reason phrase and an empty content.
        /// </summary>
        public HttpResponseMockMessage()
        {
            mock = new HttpResponseMock();
            content = new HttpResponseMockContent(mock);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseMockMessage"/> class.
        /// </summary>
        /// <param name="httpResponseMock">
        /// The mock that provides data.
        /// </param>
        public HttpResponseMockMessage(HttpResponseMock httpResponseMock)
        {
            mock = httpResponseMock;
            content = new HttpResponseMockContent(mock);
        }

        /// <summary>
        /// Return a new HttpResponseMessage.
        /// </summary>
        /// <returns>
        /// A new HttpResponseMessage.
        /// </returns>
        public HttpResponseMessage GetMessage()
        {
            var message = new HttpResponseMessage(mock.StatusCode);
            message.ReasonPhrase = mock.ReasonPhrase;
            message.Content = content.GetHttpContent();
            return message;
        }
    }
}
