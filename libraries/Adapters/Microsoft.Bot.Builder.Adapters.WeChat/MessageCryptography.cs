// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Microsoft.Bot.Builder.Adapters.WeChat.Helpers;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    /// <summary>
    /// A cryptography class to decrypt the message content from WeChat.
    /// </summary>
    public class MessageCryptography
    {
        private readonly string _token;
        private readonly string _encodingAesKey;
        private readonly string _appId;
        private readonly string _msgSignature;
        private readonly string _timestamp;
        private readonly string _nonce;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCryptography"/> class.
        /// </summary>
        /// <param name="secretInfo">The secret info provide by WeChat.</param>
        /// <param name="settings">The WeChat settings.</param>
        public MessageCryptography(SecretInfo secretInfo, WeChatSettings settings)
        {
            if (string.IsNullOrEmpty(settings.EncodingAesKey) || settings.EncodingAesKey.Length != 43)
            {
                throw new ArgumentException("Invalid EncodingAESKey.", nameof(secretInfo));
            }

            _token = settings.Token;
            _appId = settings.AppId;
            _encodingAesKey = settings.EncodingAesKey;
            _msgSignature = secretInfo.MessageSignature;
            _timestamp = secretInfo.Timestamp;
            _nonce = secretInfo.Nonce;
        }

        /// <summary>
        /// Verify the authenticity of the message and get the decrypted plaintext.
        /// </summary>
        /// <param name="postData">Cipher message.</param>
        /// <returns>Decrypted message string.</returns>
        public string DecryptMessage(string postData)
        {
            // This should fix the XXE loophole
            var doc = new XmlDocument
            {
                XmlResolver = null,
            };

            doc.LoadXml(postData);
            var root = doc.FirstChild ?? throw new ArgumentException("Invalid post data.", nameof(postData));
            var encryptMessage = root["Encrypt"]?.InnerText ?? root["encrypt"]?.InnerText ?? throw new ArgumentException("Invalid post data, no encrypted field.", nameof(postData));

            if (!VerificationHelper.VerifySignature(_msgSignature, _timestamp, _nonce, _token, encryptMessage))
            {
                throw new UnauthorizedAccessException("Signature verification failed.");
            }

            return AesDecrypt(encryptMessage, _encodingAesKey, _appId);
        }

        /// <summary>
        /// Decrypt the message.
        /// </summary>
        /// <param name="encryptString">Encrypted string.</param>
        /// <param name="encodingAesKey">Encoding AES key for decrypt message.</param>
        /// <param name="appId">The WeChat app id.</param>
        /// <returns>Decrypted string.</returns>
        private static string AesDecrypt(string encryptString, string encodingAesKey, string appId)
        {
            var key = Convert.FromBase64String(encodingAesKey + "=");
            var iv = new byte[16];
            Array.Copy(key, iv, 16);
            var btmpMsg = AesDecrypt(encryptString, iv, key);

            var len = BitConverter.ToInt32(btmpMsg, 16);
            len = IPAddress.NetworkToHostOrder(len);

            var messageBytes = new byte[len];
            var appIdBytes = new byte[btmpMsg.Length - 20 - len];
            Array.Copy(btmpMsg, 20, messageBytes, 0, len);
            Array.Copy(btmpMsg, 20 + len, appIdBytes, 0, btmpMsg.Length - 20 - len);
            var oriMsg = Encoding.UTF8.GetString(messageBytes);
            if (appId != Encoding.UTF8.GetString(appIdBytes))
            {
                throw new ArgumentException("Failed to validate appId.", nameof(appId));
            }

            return oriMsg;
        }

        /// <summary>
        /// Decode the decrypted byte array.
        /// </summary>
        /// <param name="decrypted">Decrypted byte array.</param>
        /// <returns>Decoded byte array.</returns>
        private static byte[] Decode(byte[] decrypted)
        {
            var pad = decrypted[decrypted.Length - 1];

            // This convert is comming from WeChat offical demo code.
            // https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1434696670
            if (pad < 1 || pad > 32)
            {
                pad = 0;
            }

            var result = new byte[decrypted.Length - pad];
            Array.Copy(decrypted, 0, result, 0, decrypted.Length - pad);
            return result;
        }

        /// <summary>
        /// Decrypt the AES encrypted input string.
        /// </summary>
        /// <param name="input">Encrypted string.</param>
        /// <param name="iv">Gets or sets the initialization vector for the symmetric algorithm.</param>
        /// <param name="key">Encoding Aes Key.</param>
        /// <returns>Decrypted byte array.</returns>
        private static byte[] AesDecrypt(string input, byte[] iv, byte[] key)
        {
            using (var aes = Aes.Create())
            {
                if (aes == null)
                {
                    throw new CryptographicException("Failed to create AES instance.");
                }

                // Original size is 256
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                aes.Key = key;
                aes.IV = iv;
                using (var ms = new MemoryStream())
                {
                    var decrypt = aes.CreateDecryptor(aes.Key, aes.IV);
                    using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                    {
                        var xmlBytes = Convert.FromBase64String(input);
                        cs.Write(xmlBytes, 0, xmlBytes.Length);
                    }

                    var buffArray = Decode(ms.ToArray());
                    return buffArray;
                }
            }
        }
    }
}
