using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public class MD5Hash : IAttachmentHash
    {
        /// <summary>
        /// Gets instance for MD5Hash.
        /// </summary>
        /// <value>
        /// Instance for MD5Hash.
        /// </value>
        public static MD5Hash Instance { get; } = new MD5Hash();

        /// <summary>
        /// Calculate MD5 hash, used to ignore same file when upload media.
        /// </summary>
        /// <param name="inputBytes">Bytes content need to be hashed.</param>
        /// <returns>MD5 hashed string.</returns>
        public string Hash(byte[] inputBytes)
        {
            // step 1, calculate MD5 hash from input
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(inputBytes);

                // step 2, convert byte array to hex string
                var sb = new StringBuilder();
                for (var i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Calculate MD5 hash, used to ignore same file when upload media.
        /// </summary>
        /// <param name="content">string content need to be hashed.</param>
        /// <returns>MD5 hashed string.</returns>
        public string Hash(string content)
        {
            // step 1, calculate MD5 hash from input
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(content);
                var hash = md5.ComputeHash(inputBytes);

                // step 2, convert byte array to hex string
                var sb = new StringBuilder();
                for (var i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}
