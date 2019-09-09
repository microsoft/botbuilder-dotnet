// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Tests
{
    public class MessageCryptographyTest
    {
        [Fact]
        public void DecryptMsgTest()
        {
            var postData = MockDataUtility.XmlEncrypt;
            var result = MockDataUtility.TestDecryptMsg.DecryptMessage(postData);

            // Need to remove "/r" created by editor
            var decryptString = MockDataUtility.XmlDecrypt.Replace("\r", string.Empty);
            Assert.Equal(decryptString, result);
        }

        [Fact]
        public void EncodingAESKeyTest()
        {
            var result = Assert.Throws<ArgumentException>(() => new MessageCryptography(MockDataUtility.SecretInfoAesKeyError, MockDataUtility.WeChatSettingsAesKeyError));
            Assert.Equal("Invalid EncodingAESKey.\r\nParameter name: secretInfo", result.Message);
        }

        [Fact]
        public void VerifySignatureTest()
        {
            var postData = MockDataUtility.XmlEncrypt;
            var result = Assert.Throws<UnauthorizedAccessException>(() => MockDataUtility.TestSignature.DecryptMessage(postData));
            Assert.Equal("Signature verification failed.", result.Message);
        }
    }
}
