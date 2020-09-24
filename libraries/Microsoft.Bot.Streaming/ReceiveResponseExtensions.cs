// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Streaming
{
    /// <summary>
    /// Helper methods added to the <see cref="ReceiveResponse"/> class.
    /// </summary>
    public static class ReceiveResponseExtensions
    {
        /// <summary>
        /// Serializes the body of this <see cref="ReceiveResponse"/> as JSON.
        /// </summary>
        /// <typeparam name="T">The type to attempt to deserialize the contents of this <see cref="ReceiveResponse"/>'s body into.</typeparam>
        /// <param name="response">The current instance of <see cref="ReceiveResponse"/>.</param>
        /// <returns>On success, an object of type T populated with data serialized from the <see cref="ReceiveResponse"/> body.
        /// Otherwise a default instance of type T.
        /// </returns>
        public static T ReadBodyAsJson<T>(this ReceiveResponse response)
        {
            var contentStream = response.Streams.FirstOrDefault();

            /* If the response had no body we have to return a compatible
             * but empty object to avoid throwing exceptions upstream anytime
             * an empty response is received.
             */
            if (contentStream == null)
            {
                return default;
            }

            using (var reader = new StreamReader(contentStream.Stream, Encoding.UTF8))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    var serializer = JsonSerializer.Create(SerializationSettings.DefaultDeserializationSettings);
                    return serializer.Deserialize<T>(jsonReader);
                }
            }
        }

        /// <summary>
        /// Serializes the body of this <see cref="ReceiveResponse"/> as a <see cref="string"/>.
        /// </summary>
        /// <param name="response">The current instance of <see cref="ReceiveResponse"/>.</param>
        /// <returns>On success, an <see cref="string"/> of the data from the <see cref="ReceiveResponse"/> body.
        /// </returns>
        public static string ReadBodyAsString(this ReceiveResponse response)
        {
            var contentStream = response.Streams.FirstOrDefault();

            if (contentStream != null)
            {
                return contentStream.Stream.ReadAsUtf8String();
            }

            return string.Empty;
        }
    }
}
