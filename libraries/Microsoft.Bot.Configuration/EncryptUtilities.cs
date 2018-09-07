// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration.Encryption
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    public static class EncryptUtilities
    {
        /// <summary>
        /// Generate key to use for encryption.
        /// </summary>
        /// <returns>base64 encoded cryptokey</returns>
        public static string GenerateKey()
        {
            using (var aes = AesManaged.Create())
            {
                return Convert.ToBase64String(aes.Key);
            }
        }

        /// <summary>
        /// Encrypt a string
        /// </summary>
        /// <param name="plainText">test to encrypt</param>
        /// <param name="key">key to encrypt with</param>
        /// <returns>encrypted value as Base64 string</returns>
        public static string Encrypt(this string plainText, string key)
        {
            if (plainText == null)
            {
                throw new ArgumentNullException("Missing plainText");
            }

            if (key == null)
            {
                throw new ArgumentNullException("Missing key");
            }

            // Encrypt the string to an array of bytes.
            var encrypted = EncryptUtilities.EncryptStringToBytes_Aes(plainText, key: Convert.FromBase64String(key));

            // encode iv and encrypted string as [iv]![encrypted]
            return $"{Convert.ToBase64String(encrypted.Item1)}!{Convert.ToBase64String(encrypted.Item2)}";
        }

        /// <summary>
        /// Decrypt a string.
        /// </summary>
        /// <param name="encryptedText">encrypted text</param>
        /// <param name="key">key to use to decrypt the text</param>
        /// <returns>original unecrypted value</returns>
        public static string Decrypt(this string encryptedText, string key)
        {
            if (encryptedText == null)
            {
                throw new ArgumentNullException("Missing encryptedText");
            }

            if (key == null)
            {
                throw new ArgumentNullException("Missing key");
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
        /// Stock MSDN crypto function that every single blog post on the planet uses.
        /// </summary>
        /// <param name="plainText">text to encrypt</param>
        /// <param name="key">32 byte encryption key to use</param>
        /// <param name="iv"> 16 byte iv to use</param>
        /// <returns>Tuple[IV,EncryptedBytes]</returns>
        public static Tuple<byte[], byte[]> EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv = null)
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
        /// Stock MSDN crypto function that every single blog post on the planet uses
        /// </summary>
        /// <param name="cipherText">encrypted byte array to decrypt</param>
        /// <param name="key">key to use</param>
        /// <param name="iv">iv to use</param>
        /// <returns>decrypted string</returns>
        public static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
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
