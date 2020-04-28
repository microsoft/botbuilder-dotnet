using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Bot.Builder
{
    public static class StringUtils
    {
        /// <summary>
        /// Truncate string with ...
        /// </summary>
        /// <param name="text">text.</param>
        /// <param name="length">length to truncate text.</param>
        /// <returns>string length + ...</returns>
        public static string Ellipsis(string text, int length)
        {
            if (text.Length <= length)
            {
                return text;
            }
            
            return $"{text.Substring(0, length)}...";
        }

        /// <summary>
        /// UniqueHash - create a unique hash from a string.
        /// </summary>
        /// <param name="text">text to hash.</param>
        /// <returns>string which is unique SHA256 hash.</returns>
        public static string Hash(string text)
        {
            using (var sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(text));
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// EllipsisHash - return truncated string with unique hash for the truncated part.
        /// </summary>
        /// <param name="text">text to truncate.</param>
        /// <param name="length">length to truncate at.</param>
        /// <returns>prefix up to length + ... + uniquehash(text)</returns>
        public static string EllipsisHash(string text, int length)
        {
            if (text.Length <= length)
            {
                return text;
            }

            return $"{Ellipsis(text, length)}{Hash(text)}";
        }
    }
}
