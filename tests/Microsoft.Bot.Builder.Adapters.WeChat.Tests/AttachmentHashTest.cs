using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Test
{
    [TestClass]
    public class AttachmentHashTest
    {
        [TestMethod]
        public void Md5HashTest()
        {
            var md5Hash = new MD5Hash();
            var testString = "test string to get hash";
            var stringHashed = md5Hash.Hash(testString);
            var bytesHashed = md5Hash.Hash(Encoding.UTF8.GetBytes(testString));
            Assert.AreEqual(stringHashed, "E16B52D76AD74BB8D4B507515CD9ADB8");
            Assert.AreEqual(stringHashed, bytesHashed);
        }
    }
}
