using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class AttachmentTests
    {
        [Fact]
        public void SuccessfullyInitAttachment()
        {
            var contentType = "contentType";
            var contentUrl = "contentUrl";
            var content = new { };
            var name = "name";
            var thumbnailUrl = "thumbnailUrl";
            var properties = new JObject();

            var attachment = new Attachment(contentType, contentUrl, content, name, thumbnailUrl)
            {
                Properties = properties
            };

            Assert.NotNull(attachment);
            Assert.IsType<Attachment>(attachment);
            Assert.Equal(contentType, attachment.ContentType);
            Assert.Equal(contentUrl, attachment.ContentUrl);
            Assert.Equal(content, attachment.Content);
            Assert.Equal(name, attachment.Name);
            Assert.Equal(thumbnailUrl, attachment.ThumbnailUrl);
            Assert.Equal(properties, attachment.Properties);
        }
    }
}
