// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks
{
    /// <summary>
    /// Convert and store the actual content of HttpResonseMock.
    /// </summary>
    public class HttpResponseMockContent
    {
        private readonly HttpResponseMock.ResponseContentType contentType;

        private readonly object content;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseMockContent"/> class.
        /// Contain an empty content.
        /// </summary>
        public HttpResponseMockContent()
        {
            contentType = HttpResponseMock.ResponseContentType.String;
            content = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseMockContent"/> class.
        /// </summary>
        /// <param name="httpResponseMock">The mock that provides content.</param>
        public HttpResponseMockContent(HttpResponseMock httpResponseMock)
        {
            contentType = httpResponseMock.ContentType;
            switch (contentType)
            {
                case HttpResponseMock.ResponseContentType.String:
                    content = httpResponseMock.Content == null ? string.Empty : httpResponseMock.Content.ToString();
                    break;
                case HttpResponseMock.ResponseContentType.ByteArray:
                    content = Convert.FromBase64String(httpResponseMock.Content == null ? string.Empty : httpResponseMock.Content.ToString());
                    break;
                default:
                    throw new NotSupportedException($"{httpResponseMock.ContentType} is not supported yet!");
            }
        }

        /// <summary>
        /// Return a new HttpContent based on content.
        /// </summary>
        /// <returns>A new HttpContent.</returns>
        public HttpContent GetHttpContent()
        {
            switch (contentType)
            {
                case HttpResponseMock.ResponseContentType.String:
                    return new StringContent((string)content);
                case HttpResponseMock.ResponseContentType.ByteArray:
                    return new ByteArrayContent((byte[])content);
                default:
                    throw new NotSupportedException($"{contentType} is not supported yet!");
            }
        }
    }
}
