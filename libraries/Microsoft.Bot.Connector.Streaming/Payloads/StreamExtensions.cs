// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Streaming.Payloads
{
    internal static class StreamExtensions
    {
        /// <summary>
        /// Read the contents of the stream and convert to an Utf8 string.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<string> ReadAsUtf8StringAsync(this Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Read the contents of the stream and convert to an Utf8 string.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>Stream contents as string.</returns>
        public static string ReadAsUtf8String(this Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
