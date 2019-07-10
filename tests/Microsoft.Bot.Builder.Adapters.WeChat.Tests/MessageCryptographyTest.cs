using System;
using Microsoft.Bot.Builder.Adapters.WeChat.Test.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Test
{
    [TestClass]
    public class MessageCryptographyTest
    {
        [TestMethod]
        public void DecryptMsgTest()
        {
            var postData = MockDataUtility.XmlEncrypt;
            var result = MockDataUtility.TestDecryptMsg.DecryptMessage(postData);

            // Need to remove "/r" created by editor
            var decryptString = MockDataUtility.XmlDecrypt.Replace("\r", string.Empty);
            Assert.AreEqual(decryptString, result);
        }

        [TestMethod]
        public void EncodingAESKeyTest()
        {
            var postData = MockDataUtility.XmlEncrypt;
            Assert.ThrowsException<ArgumentException>(
                () => MockDataUtility.TestAESKey.DecryptMessage(postData),
                "Invalid EncodingAESKey");
        }

        [TestMethod]
        public void VerifySignatureTest()
        {
            var postData = MockDataUtility.XmlEncrypt;
            Assert.ThrowsException<ArgumentException>(
                () => MockDataUtility.TestSignature.DecryptMessage(postData),
                "Signature validation failed.");
        }
    }
}
