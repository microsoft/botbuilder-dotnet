using System.Text;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Test
{
    public class AttachmentHashTest
    {
        [Fact]
        public void Md5HashTest()
        {
            var md5Hash = new MD5Hash();
            var testString = "test string to get hash";
            var stringHashed = md5Hash.Hash(testString);
            var bytesHashed = md5Hash.Hash(Encoding.UTF8.GetBytes(testString));
            Assert.Equal("E16B52D76AD74BB8D4B507515CD9ADB8", stringHashed);
            Assert.Equal(bytesHashed, stringHashed);
        }
    }
}
