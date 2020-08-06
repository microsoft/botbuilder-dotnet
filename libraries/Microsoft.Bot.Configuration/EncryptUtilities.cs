// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration.Encryption
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    /// <summary>
    /// Helper methods to assist with encryption of connected service keys.
    /// </summary>
    [Obsolete("This class is deprecated.  See https://aka.ms/bot-file-basics for more information.", false)]
    public static class EncryptUtilities
    {
        /// <summary>
        /// Generates a key to use for encryption.
        /// </summary>
        /// <returns>The base64-encoded cryptokey.</returns>
        public static string GenerateKey()
        {
            using (var aes = AesManaged.Create())
            {
                return Convert.ToBase64String(aes.Key);
            }
        }

        /// <summary>
        /// Encrypts a string.
        /// </summary>
        /// <param name="plainText">The string to encrypt.</param>
        /// <param name="key">The key to use for encryption.</param>
        /// <returns>The base64-encrypted value.</returns>
        public static string Encrypt(this string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            if (string.IsNullOrEmpty(key))
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly (this class is obsolete, we won't fix it)
                throw new ArgumentNullException("Missing key");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            // Encrypt the string to an array of bytes.
            var encrypted = EncryptUtilities.EncryptStringToBytes_Aes(plainText, key: Convert.FromBase64String(key));

            // encode iv and encrypted string as [iv]![encrypted]
            return $"{Convert.ToBase64String(encrypted.Item1)}!{Convert.ToBase64String(encrypted.Item2)}";
        }

        /// <summary>
        /// Decrypts a string.
        /// </summary>
        /// <param name="encryptedText">The base64-encrypted string.</param>
        /// <param name="key">The key to use for decryption.</param>
        /// <returns>The decrypted string.</returns>
        public static string Decrypt(this string encryptedText, string key)
        {
            if (string.IsNullOrEmpty(encryptedText))
            {
                return encryptedText;
            }

            if (string.IsNullOrEmpty(key))
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly (this class is obsolete, we won't fix it)
                throw new ArgumentNullException("Missing key");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            }

            var parts = encryptedText.Split('!');
            if (parts.Length != 2)
            {
                throw new ArgumentException("EncryptedText is not properly formatted");
            }

            byte[] bytesIv;
            try
            {
                bytesIv = Convert.FromBase64String(parts[0]);
            }
            catch (FormatException)
            {
                throw new ArgumentException("EncryptedText[0] is not properly formatted");
            }

            byte[] cipherText;
            try
            {
                cipherText = Convert.FromBase64String(parts[1]);
            }
            catch (FormatException)
            {
                throw new ArgumentException("EncryptedText[1] is not properly formatted");
            }

            byte[] bytesKey;
            try
            {
                bytesKey = Convert.FromBase64String(key);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Key is not properly formatted");
            }

            // parts[0] == base64 iv parts[1] == base64 encoded encrypted bytes
            return EncryptUtilities.DecryptStringFromBytes_Aes(cipherText: cipherText, key: bytesKey, iv: bytesIv);
        }

        /// <summary>
        /// Encrypts a string using Advanced Encryption Standard (AES).
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="key">The 32-byte encryption key to use.</param>
        /// <param name="iv">A 16-byte initialization vector to use.</param>
        /// <returns>The initialization vector and the encrypted bytes.</returns>
#pragma warning disable CA1707 // Identifiers should not contain underscores (this class is obsolete, we won't fix it)
        public static Tuple<byte[], byte[]> EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv = null)
#pragma warning restore CA1707 // Identifiers should not contain underscores
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException(nameof(plainText));
            }

            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException(nameof(key));
            }

            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (var aes = AesManaged.Create())
            {
                aes.Key = key;
                if (iv != null)
                {
                    // if custom iv use that
                    aes.IV = iv;
                }

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                // Create the streams used for encryption.
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (var cryptoWriter = new StreamWriter(cryptoStream))
                        {
                            // Write all data to the stream.
                            cryptoWriter.Write(plainText);
                        }

                        encrypted = memoryStream.ToArray();
                    }
                }

                // Return the encrypted bytes from the memory stream.
                return new Tuple<byte[], byte[]>(aes.IV, encrypted);
            }
        }

        /// <summary>
        /// Decrypts a string using Advanced Encryption Standard (AES).
        /// </summary>
        /// <param name="cipherText">The encrypted bytes.</param>
        /// <param name="key">The 32-byte encryption key to use.</param>
        /// <param name="iv">A 16-byte initialization vector to use.</param>
        /// <returns>The decrypted string.</returns>
#pragma warning disable CA1707 // Identifiers should not contain underscores (this class is obsolete, we won't fix it)
        public static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
#pragma warning restore CA1707 // Identifiers should not contain underscores
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException(nameof(cipherText));
            }

            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException(nameof(iv));
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (var aes = AesManaged.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                // Create the streams used for decryption.
                using (MemoryStream memoryStream = new MemoryStream(cipherText))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader decryptReader = new StreamReader(cryptoStream))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = decryptReader.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}
