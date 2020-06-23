// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using static Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks.HttpResponseMock;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks
{
    public class HttpResponseMockContent
    {
        private readonly ContentTypes contentType;

        private readonly object content;

        public HttpResponseMockContent()
        {
            contentType = ContentTypes.String;
            content = string.Empty;
        }

        public HttpResponseMockContent(HttpResponseMock httpResponseMock)
        {
            contentType = httpResponseMock.ContentType;
            switch (contentType)
            {
                case ContentTypes.String:
                    content = httpResponseMock.Content == null ? string.Empty : httpResponseMock.Content.ToString();
                    break;
                case ContentTypes.ByteArray:
                    content = Convert.FromBase64String(httpResponseMock.Content == null ? string.Empty : httpResponseMock.Content.ToString());
                    break;
                default:
                    throw new NotSupportedException($"{httpResponseMock.ContentType} is not supported yet!");
            }
        }

        public HttpContent GetHttpContent()
        {
            switch (contentType)
            {
                case ContentTypes.String:
                    return new StringContent((string)content);
                case ContentTypes.ByteArray:
                    return new ByteArrayContent((byte[])content);
                default:
                    throw new NotSupportedException($"{contentType} is not supported yet!");
            }
        }
    }
}
