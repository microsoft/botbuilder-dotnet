// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.Bot.StreamingExtensions
{
    /// <summary>
    /// Implementation split between Request and RequestEx.
    /// The basic request type sent over Bot Framework Protocol 3 with Streaming Extensions transports,
    /// equivalent to HTTP request messages.
    /// </summary>
    public partial class Request
    {
        /// <summary>
        /// Verb used by requests to get resources hosted on a remote server.
        /// </summary>
        public const string GET = "GET";

        /// <summary>
        /// Verb used by requests posting data to a remote server.
        /// </summary>
        public const string POST = "POST";

        /// <summary>
        /// Verb used by requests putting updated data on a remote server.
        /// </summary>
        public const string PUT = "PUT";

        /// <summary>
        /// Verb used by requests to delete data hosted on a remote server.
        /// </summary>
        public const string DELETE = "DELETE";

        /// <summary>
        /// Creates a <see cref="Request"/> to get resources hosted on a remote server.
        /// </summary>
        /// <param name="path">Optional path where the resource can be found on the remote server.</param>
        /// <param name="body">Optional body to send to the remote server.</param>
        /// <returns>A <see cref="Request"/> with appropriate status code and body.</returns>
        public static Request CreateGet(string path = null, HttpContent body = null)
        {
            return CreateRequest(GET, path, body);
        }

        /// <summary>
        /// Creates a <see cref="Request"/> to post data to a remote server.
        /// </summary>
        /// <param name="path">Optional path where the resource can be found on the remote server.</param>
        /// <param name="body">Optional body to send to the remote server.</param>
        /// <returns>A <see cref="Request"/> with appropriate status code and body.</returns>
        public static Request CreatePost(string path = null, HttpContent body = null)
        {
            return CreateRequest(POST, path, body);
        }

        /// <summary>
        /// Creates a <see cref="Request"/> to put updated data on a remote server.
        /// </summary>
        /// <param name="path">Optional path where the resource can be found on the remote server.</param>
        /// <param name="body">Optional body to send to the remote server.</param>
        /// <returns>A <see cref="Request"/> with appropriate status code and body.</returns>
        public static Request CreatePut(string path = null, HttpContent body = null)
        {
            return CreateRequest(PUT, path, body);
        }

        /// <summary>
        /// Creates a <see cref="Request"/> to delete data hosted on a remote server.
        /// </summary>
        /// <param name="path">Optional path where the resource can be found on the remote server.</param>
        /// <param name="body">Optional body to send to the remote server.</param>
        /// <returns>A <see cref="Request"/> with appropriate status code and body.</returns>
        public static Request CreateDelete(string path = null, HttpContent body = null)
        {
            return CreateRequest(DELETE, path, body);
        }

        /// <summary>
        /// Creates a <see cref="Request"/> with the passed in method, path, and body.
        /// </summary>
        /// <param name="method">The HTTP verb to use for this request.</param>
        /// <param name="path">Optional path where the resource can be found on the remote server.</param>
        /// <param name="body">Optional body to send to the remote server.</param>
        /// <returns>On success returns a <see cref="Request"/> with appropriate status code and body, otherwise returns null.</returns>
        public static Request CreateRequest(string method, string path = null, HttpContent body = null)
        {
            if (string.IsNullOrWhiteSpace(method))
            {
                return null;
            }

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

        /// <summary>
        /// Adds a new stream attachment to this <see cref="Request"/>.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> to include in the new stream attachment.</param>
        public void AddStream(HttpContent content) => AddStream(content, Guid.NewGuid());

        /// <summary>
        /// Adds a new stream attachment to this <see cref="Request"/>.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> to include in the new stream attachment.</param>
        /// <param name="streamId">The id to assign to this stream attachment.</param>
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
