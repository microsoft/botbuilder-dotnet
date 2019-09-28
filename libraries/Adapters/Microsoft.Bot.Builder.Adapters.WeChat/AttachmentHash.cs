// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    internal class AttachmentHash : IAttachmentHash
    {
        /// <summary>
        /// Calculates the hash value, used to ignore same file when upload media.
        /// </summary>
        /// <param name="inputBytes">Bytes content need to be hashed.</param>
        /// <returns>Hash value.</returns>
        public string ComputeHash(byte[] inputBytes)
        {
            // step 1, calculate MD5 hash from input
#pragma warning disable CA5351 // Only used to get a unique hash value
            using (var md5 = MD5.Create())
#pragma warning restore CA5351 // Only used to get a unique hash value
            {
                var hash = md5.ComputeHash(inputBytes);

                // step 2, convert byte array to hex string
                var sb = new StringBuilder();
                for (var i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2", CultureInfo.InvariantCulture));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Calculates the hash value, used to ignore same file when upload media.
        /// </summary>
        /// <param name="content">String content need to be hashed.</param>
        /// <returns>Hash value.</returns>
        public string ComputeHash(string content)
        {
            var inputBytes = Encoding.UTF8.GetBytes(content);
            return ComputeHash(inputBytes);
        }
    }
}
