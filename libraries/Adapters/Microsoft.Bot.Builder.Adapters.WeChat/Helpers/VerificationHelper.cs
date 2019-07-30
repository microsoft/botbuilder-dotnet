using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public class VerificationHelper
    {
        /// <summary>
        /// Check the uncrypt message's signature.
        /// </summary>
        /// <param name="signature">WeChat message signature in query params.</param>
        /// <param name="timestamp">WeChat message timestamp in query params.</param>
        /// <param name="nonce">WeChat message nonce in query params.</param>
        /// <param name="token">validation token from WeChat.</param>
        /// <returns>Check result.</returns>
        public static bool Check(string signature, string timestamp, string nonce, string token = null)
        {
            // token can be null when user did not set its value.
            if (string.IsNullOrEmpty(signature))
            {
                Console.WriteLine("Request validation failed - null Signature");
                return false;
            }

            if (string.IsNullOrEmpty(timestamp))
            {
                Console.WriteLine("Request validation failed - null Timestamp");
                return false;
            }

            if (string.IsNullOrEmpty(nonce))
            {
                Console.WriteLine("Request validation failed - null Nonce");
                return false;
            }

            if (!VerifySignature(signature, token, timestamp, nonce))
            {
                Console.WriteLine("Request validation failed - Signature validation faild.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verify WeChat request message signature.
        /// </summary>
        /// <param name="signature">WeChat message signature in query params.</param>
        /// <param name="token">validation token from WeChat.</param>
        /// <param name="timestamp">WeChat message timestamp in query params.</param>
        /// <param name="nonce">WeChat message nonce in query params.</param>
        /// <param name="postBody">Request body as string.</param>
        /// <returns>Signature verification result.</returns>
        public static bool VerifySignature(string signature, string token, string timestamp, string nonce, string postBody = null)
        {
            try
            {
                if (postBody == null)
                {
                    return signature == GenarateSignature(token, timestamp, nonce);
                }

                return signature == GenarateSignature(token, timestamp, nonce, postBody);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Genarate signature with no message body.
        /// </summary>
        /// <param name="token">Token in app settings.</param>
        /// <param name="timestamp">WeChat message timestamp in query params.</param>
        /// <param name="nonce">WeChat message nonce in query params.</param>
        /// <returns>Genarateed signature.</returns>
        private static string GenarateSignature(string token, string timestamp, string nonce)
        {
            var arr = new[] { token, timestamp, nonce }.OrderBy(z => z).ToArray();
            var arrString = string.Join(string.Empty, arr);
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(arrString));
                var signtureBuilder = new StringBuilder();
                foreach (var bytes in hash)
                {
                    signtureBuilder.AppendFormat("{0:x2}", bytes);
                }

                return signtureBuilder.ToString();
            }
        }

        /// <summary>
        /// Generate signature use the encrypted message.
        /// </summary>
        /// <param name="token">Token in app settings.</param>
        /// <param name="timestamp">WeChat message timestamp in query params.</param>
        /// <param name="nonce">WeChat message nonce in query params.</param>
        /// <param name="encryptedMessage">The encrypted message content from WeChat request.</param>
        /// <returns>Genarateed signature.</returns>
        private static string GenarateSignature(string token, string timestamp, string nonce, string encryptedMessage)
        {
            var arrayList = new ArrayList
            {
                token,
                timestamp,
                nonce,
                encryptedMessage,
            };
            arrayList.Sort(new DictionarySort());
            var raw = string.Empty;
            for (var i = 0; i < arrayList.Count; ++i)
            {
                raw += arrayList[i];
            }

            try
            {
                using (var sha = SHA1.Create())
                {
                    var enc = new ASCIIEncoding();
                    var dataToHash = enc.GetBytes(raw);
                    var dataHashed = sha.ComputeHash(dataToHash);
                    return BitConverter.ToString(dataHashed).Replace("-", string.Empty).ToLower();
                }
            }
            catch
            {
                throw new Exception("Compare signature failed.");
            }
        }
    }
}
