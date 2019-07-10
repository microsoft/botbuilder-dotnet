using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public class MessageCryptography
    {
        private readonly string token;
        private readonly string encodingAESKey;
        private readonly string appId;
        private readonly string msgSignature;
        private readonly string timestamp;
        private readonly string nonce;

        public MessageCryptography(SecretInfo secretInfo)
        {
            this.token = secretInfo.Token;
            this.appId = secretInfo.AppId;
            this.encodingAESKey = secretInfo.EncodingAESKey;
            this.msgSignature = secretInfo.Msg_Signature;
            this.timestamp = secretInfo.Timestamp;
            this.nonce = secretInfo.Nonce;
        }

        /// <summary>
        /// Verify the authenticity of the message and get the decrypted plaintext.
        /// </summary>
        /// <param name="postData">cipher message.</param>
        /// <returns>Decrypted message string.</returns>
        public string DecryptMessage(string postData)
        {
            if (this.encodingAESKey.Length != 43)
            {
                throw new ArgumentException("Invalid EncodingAESKey");
            }

            // This should fix the XXE loophole
            var doc = new XmlDocument
            {
                XmlResolver = null,
            };
            XmlNode root;
            string encryptMessage;
            try
            {
                doc.LoadXml(postData);
                root = doc.FirstChild;
                encryptMessage = root["Encrypt"].InnerText;
            }
            catch (Exception)
            {
                throw new ArgumentException("Failed to parse xml document.");
            }

            if (!VerificationHelper.VerifySignature(this.msgSignature, this.token, this.timestamp, this.nonce, encryptMessage))
            {
                throw new ArgumentException("Signature validation failed.");
            }

            return this.AESDecrypt(encryptMessage, this.encodingAESKey, this.appId);
        }

        /// <summary>
        /// Decrypt the message.
        /// </summary>
        /// <param name="encryptString">Encrypted string.</param>
        /// <param name="encodingAESKey">Encoding AES key for descrypt message.</param>
        /// <param name="appId">The WeChat app id.</param>
        /// <returns>Decrypted string.</returns>
        public string AESDecrypt(string encryptString, string encodingAESKey, string appId)
        {
            try
            {
                var key = Convert.FromBase64String(encodingAESKey + "=");
                var iv = new byte[16];
                Array.Copy(key, iv, 16);
                var btmpMsg = this.AESDecrypt(encryptString, iv, key);

                var len = BitConverter.ToInt32(btmpMsg, 16);
                len = IPAddress.NetworkToHostOrder(len);

                var messageBytes = new byte[len];
                var appIdBytes = new byte[btmpMsg.Length - 20 - len];
                Array.Copy(btmpMsg, 20, messageBytes, 0, len);
                Array.Copy(btmpMsg, 20 + len, appIdBytes, 0, btmpMsg.Length - 20 - len);
                var oriMsg = Encoding.UTF8.GetString(messageBytes);
                if (appId != Encoding.UTF8.GetString(appIdBytes))
                {
                    throw new ArgumentException("Failed to validate appId.");
                }

                return oriMsg;
            }
            catch (FormatException)
            {
                throw new ArgumentException("Failed to decode base64 string.");
            }
        }

        private byte[] AESDecrypt(string input, byte[] iv, byte[] key)
        {
            var aes = Aes.Create();

            // Origial size is 256
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            aes.Key = key;
            aes.IV = iv;
            var decrypt = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] buffArray = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                {
                    var xmlBytes = Convert.FromBase64String(input);
                    cs.Write(xmlBytes, 0, xmlBytes.Length);
                }

                buffArray = this.Decode(ms.ToArray());
            }

            return buffArray;
        }

        private byte[] Decode(byte[] decrypted)
        {
            var pad = decrypted[decrypted.Length - 1];
            if (pad < 1 || pad > 32)
            {
                pad = 0;
            }

            var result = new byte[decrypted.Length - pad];
            Array.Copy(decrypted, 0, result, 0, decrypted.Length - pad);
            return result;
        }
    }
}
