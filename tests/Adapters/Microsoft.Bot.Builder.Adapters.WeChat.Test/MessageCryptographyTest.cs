using System;
using Microsoft.Bot.Builder.Adapters.WeChat.Test.TestUtilities;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Test
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
            var postData = MockDataUtility.XmlEncrypt;
            var result = Assert.Throws<ArgumentException>(() => MockDataUtility.TestAESKey.DecryptMessage(postData));
            Assert.Equal("Invalid EncodingAESKey", result.Message);
        }

        [Fact]
        public void VerifySignatureTest()
        {
            var postData = MockDataUtility.XmlEncrypt;
            var result = Assert.Throws<UnauthorizedAccessException>(() => MockDataUtility.TestSignature.DecryptMessage(postData));
            Assert.Equal("Signature validation failed.", result.Message);
        }
    }
}
