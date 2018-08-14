// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration.Encryption
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public static class EncryptUtilities
    {
        /// <summary>
        /// Encrypt a string
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="secret">secret to use as the key (uses SHA256 hash to generate key)</param>
        /// <param name="iv">optional IV to salt value, first 16 chars will be used padded with spaces</param>
        /// <returns>encrypted value as Base64 string</returns>
        public static string Encrypt(this string plainText, string secret, string iv = null)
        {
            if (plainText == null)
            {
                throw new ArgumentNullException("Missing plainText");
            }

            if (secret == null)
            {
                throw new ArgumentNullException("Missing secret");
            }

            using (Aes myAes = AesManaged.Create())
            {
                if (iv == null)
                {
                    iv = string.Empty.PadRight(16);
                }
                else
                {
                    iv = iv.PadRight(16).Substring(0, 16);
                }

                myAes.IV = Encoding.UTF8.GetBytes(iv);
                myAes.Key = SHA256Managed.Create().ComputeHash(Encoding.UTF8.GetBytes(secret));

                // Encrypt the string to an array of bytes.
                byte[] encrypted = EncryptUtilities.EncryptStringToBytes_Aes(plainText, myAes.Key, myAes.IV);
                return Convert.ToBase64String(encrypted);
            }
        }

        /// <summary>
        /// Decrypt a string
        /// </summary>
        /// <param name="encryptedText">encrypted text</param>
        /// <param name="secret">secret to use as the key (uses SHA256 hash to generate key)</param>
        /// <param name="iv">optional IV to salt value, first 16 chars will be used padded with spaces</param>
        /// <returns>original unecrypted value</returns>
        public static string Decrypt(this string encryptedText, string secret, string iv = null)
        {
            if (encryptedText == null)
            {
                throw new ArgumentNullException("Missing encryptedText");
            }

            if (secret == null)
            {
                throw new ArgumentNullException("Missing secret");
            }

            using (Aes aes = AesManaged.Create())
            {
                if (iv == null)
                {
                    iv = String.Empty.PadRight(16);
                }
                else
                {
                    iv = iv.PadRight(16).Substring(0, 16);
                }

                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Key = SHA256Managed.Create().ComputeHash(Encoding.UTF8.GetBytes(secret));
                var encrypted = Convert.FromBase64String(encryptedText);
                return EncryptUtilities.DecryptStringFromBytes_Aes(encrypted, aes.Key, aes.IV);
            }
        }

        /// <summary>
        /// Stock MSDN crypto function that every single blog post on the planet uses
        /// </summary>
        /// <returns></returns>
        public static byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv)
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

            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException(nameof(iv));
            }

            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }

                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        /// <summary>
        /// Stock MSDN crypto function that every single blog post on the planet uses
        /// </summary>
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
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}
