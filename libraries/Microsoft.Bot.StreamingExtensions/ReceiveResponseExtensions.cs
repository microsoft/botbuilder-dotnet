// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.StreamingExtensions
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
            if (contentStream != null)
            {
                var stream = contentStream.GetStream();
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        var serializer = JsonSerializer.Create(SerializationSettings.DefaultDeserializationSettings);
                        return serializer.Deserialize<T>(jsonReader);
                    }
                }
            }

            return default(T);
        }

        /// <summary>
        /// Serializes the body of this <see cref="ReceiveResponse"/> as a <see cref="string"/>.
        /// </summary>
        /// <param name="response">The current instance of <see cref="ReceiveResponse"/>.</param>
        /// <returns>On success, an <see cref="string"/> of the data from the <see cref="ReceiveResponse"/> body.
        /// Otherwise <see cref="null"/>.
        /// </returns>
        public static string ReadBodyAsString(this ReceiveResponse response)
        {
            var contentStream = response.Streams.FirstOrDefault();

            if (contentStream != null)
            {
                return contentStream.GetStream().ReadAsUtf8String();
            }

            return null;
        }

        /// <summary>
        /// Serializes the body of this <see cref="ReceiveResponse"/> as a <see cref="string"/>.
        /// as an asynchronus <see cref="Task"/>.
        /// </summary>
        /// <param name="response">The current instance of <see cref="ReceiveResponse"/>.</param>
        /// <returns>On success, a <see cref="Task"/> that will provide a <see cref="string"/> of the data from the <see cref="ReceiveResponse"/> body.
        /// Otherwise <see cref="null"/>.
        /// </returns>
        private static async Task<string> ReadBodyAsStringAsync(this ReceiveResponse response)
        {
            var contentStream = response.Streams.FirstOrDefault();

            if (contentStream != null)
            {
                return await contentStream.GetStream().ReadAsUtf8StringAsync().ConfigureAwait(false);
            }

            return null;
        }
    }
}
