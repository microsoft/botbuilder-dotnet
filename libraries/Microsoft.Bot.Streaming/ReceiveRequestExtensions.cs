// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Streaming
{
    /// <summary>
    /// Helper methods added to the <see cref="ReceiveRequest"/> class.
    /// </summary>
    public static class ReceiveRequestExtensions
    {
        /// <summary>
        /// Serializes the body of this <see cref="ReceiveRequest"/> as JSON.
        /// </summary>
        /// <typeparam name="T">The type to attempt to deserialize the contents of this <see cref="ReceiveRequest"/>'s body into.</typeparam>
        /// <param name="request">The current instance of <see cref="ReceiveRequest"/>.</param>
        /// <returns>On success, an object of type T populated with data serialized from the <see cref="ReceiveRequest"/> body.
        /// Otherwise a default instance of type T.
        /// </returns>
        public static T ReadBodyAsJson<T>(this ReceiveRequest request)
        {
            return request.ReadBodyAsJsonAsync<T>().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Serializes the body of this <see cref="ReceiveRequest"/> as JSON.
        /// </summary>
        /// <typeparam name="T">The type to attempt to deserialize the contents of this <see cref="ReceiveRequest"/>'s body into.</typeparam>
        /// <param name="request">The current instance of <see cref="ReceiveRequest"/>.</param>
        /// <returns>On success, an object of type T populated with data serialized from the <see cref="ReceiveRequest"/> body.
        /// Otherwise a default instance of type T.
        /// </returns>
        public static async Task<T> ReadBodyAsJsonAsync<T>(this ReceiveRequest request)
        {
            // The first stream attached to a ReceiveRequest is always the ReceiveRequest body.
            // Any additional streams must be defined within the body or they will not be
            // attached properly when processing activities.
            var contentStream = request.Streams.FirstOrDefault();

            /* If the response had no body we have to return a compatible
            * but empty object to avoid throwing exceptions upstream anytime
            * an empty response is received.
            */
            if (contentStream == null)
            {
                return default;
            }

            var bodyString = await request.ReadBodyAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<T>(bodyString, SerializationSettings.DefaultDeserializationSettings);
        }

        /// <summary>
        /// Reads the body of this <see cref="ReceiveRequest"/> as a string.
        /// </summary>
        /// <param name="request">The current instance of <see cref="ReceiveRequest"/>.</param>
        /// <returns>On success, a string populated with data read from the <see cref="ReceiveRequest"/> body.
        /// Otherwise null.
        /// </returns>
        public static string ReadBodyAsString(this ReceiveRequest request)
        {
            return request.ReadBodyAsStringAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Reads the body of this <see cref="ReceiveRequest"/> as a string.
        /// </summary>
        /// <param name="request">The current instance of <see cref="ReceiveRequest"/>.</param>
        /// <returns>On success, a string populated with data read from the <see cref="ReceiveRequest"/> body.
        /// Otherwise null.
        /// </returns>
        public static Task<string> ReadBodyAsStringAsync(this ReceiveRequest request)
        {
            var contentStream = request.Streams.FirstOrDefault();

            if (contentStream == null)
            {
                return Task.FromResult(string.Empty);
            }

            using (var reader = new StreamReader(contentStream.Stream, Encoding.UTF8))
            {
                return reader.ReadToEndAsync();
            }
        }
    }
}
