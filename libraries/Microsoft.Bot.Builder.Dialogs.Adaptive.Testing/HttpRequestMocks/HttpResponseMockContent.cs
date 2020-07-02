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
        private readonly HttpResponseMock.ContentTypes contentType;

        private readonly object content;

        public HttpResponseMockContent()
        {
            contentType = HttpResponseMock.ContentTypes.String;
            content = string.Empty;
        }

        public HttpResponseMockContent(HttpResponseMock httpResponseMock)
        {
            contentType = httpResponseMock.ContentType;
            switch (contentType)
            {
                case HttpResponseMock.ContentTypes.String:
                    content = httpResponseMock.Content == null ? string.Empty : httpResponseMock.Content.ToString();
                    break;
                case HttpResponseMock.ContentTypes.ByteArray:
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
                case HttpResponseMock.ContentTypes.String:
                    return new StringContent((string)content);
                case HttpResponseMock.ContentTypes.ByteArray:
                    return new ByteArrayContent((byte[])content);
                default:
                    throw new NotSupportedException($"{contentType} is not supported yet!");
            }
        }
    }
}
