// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Helpers
{
    public static class VerificationHelper
    {
        /// <summary>
        /// Verify WeChat request message signature.
        /// </summary>
        /// <param name="signature">WeChat message signature in query params.</param>
        /// <param name="timestamp">WeChat message timestamp in query params.</param>
        /// <param name="nonce">WeChat message nonce in query params.</param>
        /// <param name="token">Validation token from WeChat.</param>
        /// <param name="postBody">Request body as string.</param>
        /// <returns>Signature verification result.</returns>
        public static bool VerifySignature(string signature, string timestamp, string nonce, string token = null, string postBody = null)
        {
            // token can be null when user did not set its value.
            if (string.IsNullOrEmpty(signature))
            {
                throw new ArgumentException("Request validation failed - null Signature", nameof(signature));
            }

            if (string.IsNullOrEmpty(timestamp))
            {
                throw new ArgumentException("Request validation failed - null Timestamp", nameof(timestamp));
            }

            if (string.IsNullOrEmpty(nonce))
            {
                throw new ArgumentException("Request validation failed - null Nonce", nameof(nonce));
            }

            return signature == GenerateSignature(token, timestamp, nonce, postBody);
        }

        /// <summary>
        /// Generate signature use the encrypted message.
        /// </summary>
        /// <param name="token">Token in app settings.</param>
        /// <param name="timestamp">WeChat message timestamp in query params.</param>
        /// <param name="nonce">WeChat message nonce in query params.</param>
        /// <param name="encryptedMessage">The encrypted message content from WeChat request.</param>
        /// <returns>Generated signature.</returns>
        private static string GenerateSignature(string token, string timestamp, string nonce, string encryptedMessage = null)
        {
            var arr = string.IsNullOrEmpty(encryptedMessage) ? new[] { token, timestamp, nonce } : new[] { token, timestamp, nonce, encryptedMessage };
            Array.Sort(arr, Compare);
            var raw = string.Join(string.Empty, arr);

#pragma warning disable CA5350

            // WeChat use SHA1 to generate signature.
            // https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421135319
            using (var sha1 = SHA1.Create())
#pragma warning restore CA5350
            {
                var dataToHash = Encoding.ASCII.GetBytes(raw);
                var dataHashed = sha1.ComputeHash(dataToHash);
                var signtureBuilder = new StringBuilder();
                foreach (var bytes in dataHashed)
                {
                    signtureBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", bytes);
                }

                return signtureBuilder.ToString();
            }
        }

        /// <summary>
        /// Compare method used to sort the secret info used to generate signature.
        /// </summary>
        /// <param name="left">Left string to be compared.</param>
        /// <param name="right">Right string to be compared.</param>
        /// <returns>Int value indicate compare result.</returns>
        private static int Compare(string left, string right)
        {
            var leftLength = left.Length;
            var rightLength = right.Length;
            var index = 0;
            while (index < leftLength && index < rightLength)
            {
                if (left[index] < right[index])
                {
                    return -1;
                }
                else if (left[index] > right[index])
                {
                    return 1;
                }
                else
                {
                    index++;
                }
            }

            return leftLength - rightLength;
        }
    }
}
