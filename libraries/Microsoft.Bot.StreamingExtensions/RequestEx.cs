// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.Bot.StreamingExtensions
{
    public partial class Request
    {
        public const string GET = "GET";
        public const string POST = "POST";
        public const string PUT = "PUT";
        public const string DELETE = "DELETE";

        public static Request CreateGet(string path = null, HttpContent body = null)
        {
            return CreateRequest(GET, path, body);
        }

        public static Request CreatePost(string path = null, HttpContent body = null)
        {
            return CreateRequest(POST, path, body);
        }

        public static Request CreatePut(string path = null, HttpContent body = null)
        {
            return CreateRequest(PUT, path, body);
        }

        public static Request CreateDelete(string path = null, HttpContent body = null)
        {
            return CreateRequest(DELETE, path, body);
        }

        public static Request CreateRequest(string method, string path = null, HttpContent body = null)
        {
            var request = new Request()
            {
                Verb = method,
                Path = path,
            };

            if (body != null)
            {
                request.AddStream(body);
            }

            return request;
        }

        public void AddStream(HttpContent content) => AddStream(content, Guid.NewGuid());

        private void AddStream(HttpContent content, Guid streamId)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (Streams == null)
            {
                Streams = new List<HttpContentStream>();
            }

            Streams.Add(
                new HttpContentStream(streamId)
                {
                    Content = content,
                });
        }
    }
}
