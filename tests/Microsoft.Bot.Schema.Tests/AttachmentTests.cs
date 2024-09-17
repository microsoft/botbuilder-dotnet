// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class AttachmentTests
    {
        [Fact]
        public void AttachmentInits()
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

        [Fact]
        public void AttachmentDataInits()
        {
            var type = "type";
            var name = "name";
            var originalBase64 = new byte[0];
            var thumbnailBase64 = new byte[0];

            var attachmentData = new AttachmentData(type, name, originalBase64, thumbnailBase64);

            Assert.NotNull(attachmentData);
            Assert.IsType<AttachmentData>(attachmentData);
            Assert.Equal(type, attachmentData.Type);
            Assert.Equal(name, attachmentData.Name);
            Assert.Equal(originalBase64, attachmentData.OriginalBase64);
            Assert.Equal(thumbnailBase64, attachmentData.ThumbnailBase64);
        }

        [Fact]
        public void AttachmentDataInitsWithNoArgs()
        {
            var attachmentData = new AttachmentData();

            Assert.NotNull(attachmentData);
            Assert.IsType<AttachmentData>(attachmentData);
        }

        [Fact]
        public void AttachmentInfoInits()
        {
            var name = "name";
            var type = "type";
            var views = new List<AttachmentView>() { new AttachmentView() };

            var attachmentInfo = new AttachmentInfo(name, type, views);

            Assert.NotNull(attachmentInfo);
            Assert.IsType<AttachmentInfo>(attachmentInfo);
            Assert.Equal(name, attachmentInfo.Name);
            Assert.Equal(type, attachmentInfo.Type);
            Assert.Equal(views, attachmentInfo.Views);
        }

        [Fact]
        public void AttachmentInfoInitsWithNoArgs()
        {
            var attachmentInfo = new AttachmentInfo();

            Assert.NotNull(attachmentInfo);
            Assert.IsType<AttachmentInfo>(attachmentInfo);
        }

        [Fact]
        public void AttachmentViewInits()
        {
            var viewId = "viewId";
            var size = 5;

            var attachmentView = new AttachmentView(viewId, size);

            Assert.NotNull(attachmentView);
            Assert.Equal(viewId, attachmentView.ViewId);
            Assert.Equal(size, attachmentView.Size);
        }

        [Fact]
        public void AttachmentShouldWorkWithoutJsonConverter()
        {
            var text = "Hi!";
            var activity = new ActivityDummy
            {
                Attachments = new Attachment[]
                {
                    new AttachmentDummy { ContentType = "string", Content = text },
                    new AttachmentDummy { ContentType = "string/array", Content = new string[] { text } },
                    new AttachmentDummy { ContentType = "dict", Content = new Dictionary<string, object> { { "firstname", "John" }, { "attachment1", new AttachmentDummy(content: text) }, { "lastname", "Doe" }, { "attachment2", new AttachmentDummy(content: text) }, { "age", 18 } } },
                    new AttachmentDummy { ContentType = "attachment", Content = new AttachmentDummy(content: text) },
                    new AttachmentDummy { ContentType = "attachment/dict", Content = new Dictionary<string, AttachmentDummy> { { "attachment", new AttachmentDummy(content: text) }, { "attachment2", new AttachmentDummy(content: text) } } },
                    new AttachmentDummy { ContentType = "attachment/dict/nested", Content = new Dictionary<string, Dictionary<string, AttachmentDummy>> { { "attachment", new Dictionary<string, AttachmentDummy> { { "content", new AttachmentDummy(content: text) } } } } },
                    new AttachmentDummy { ContentType = "attachment/list", Content = new List<AttachmentDummy> { new AttachmentDummy(content: text), new AttachmentDummy(content: text) } },
                    new AttachmentDummy { ContentType = "attachment/list/nested", Content = new List<List<AttachmentDummy>> { new List<AttachmentDummy> { new AttachmentDummy(content: text) } } },
                }
            };

            AssertAttachment(activity);
        }

        [Fact]
        public void AttachmentShouldWorkWithJsonConverter()
        {
            var text = "Hi!";
            var activity = new Activity
            {
                Attachments = new Attachment[]
                {
                    new Attachment { ContentType = "string", Content = text },
                    new Attachment { ContentType = "string/array", Content = new string[] { text } },
                    new Attachment { ContentType = "dict", Content = new Dictionary<string, object> { { "firstname", "John" }, { "attachment1", new Attachment(content: text) }, { "lastname", "Doe" }, { "attachment2", new Attachment(content: text) }, { "age", 18 } } },
                    new Attachment { ContentType = "attachment", Content = new Attachment(content: text) },
                    new Attachment { ContentType = "attachment/dict", Content = new Dictionary<string, Attachment> { { "attachment", new Attachment(content: text) } } },
                    new Attachment { ContentType = "attachment/dict/nested", Content = new Dictionary<string, Dictionary<string, Attachment>> { { "attachment", new Dictionary<string, Attachment> { { "content", new Attachment(content: text) } } } } },
                    new Attachment { ContentType = "attachment/list", Content = new List<Attachment> { new Attachment(content: text), new Attachment(content: text) } },
                    new Attachment { ContentType = "attachment/list/nested", Content = new List<List<Attachment>> { new List<Attachment> { new Attachment(content: text) } } },
                }
            };

            AssertAttachment(activity);
        }

        [Fact]
        public void MemoryStreamAttachmentShouldWorkWithJsonConverter()
        {
            var text = "Hi!";
            var buffer = Encoding.UTF8.GetBytes(text);
            var activity = new Activity
            {
                Attachments = new Attachment[]
                {
                    new Attachment { ContentType = "stream", Content = new MemoryStream(buffer) },
                    new Attachment { ContentType = "stream/empty", Content = new MemoryStream() },
                    new Attachment { ContentType = "stream/dict", Content = new Dictionary<string, MemoryStream> { { "stream", new MemoryStream(buffer) } } },
                    new Attachment { ContentType = "stream/dict/nested", Content = new Dictionary<string, Dictionary<string, MemoryStream>> { { "stream", new Dictionary<string, MemoryStream> { { "content", new MemoryStream(buffer) } } } } },
                    new Attachment { ContentType = "stream/list", Content = new List<MemoryStream> { new MemoryStream(buffer), new MemoryStream(buffer) } },
                    new Attachment { ContentType = "stream/list/nested", Content = new List<List<MemoryStream>> { new List<MemoryStream> { new MemoryStream(buffer) } } },
                }
            };

            var serialized = JsonConvert.SerializeObject(activity, new JsonSerializerSettings { MaxDepth = null });
            var deserialized = JsonConvert.DeserializeObject<Activity>(serialized);

            var buffer0 = (GetAttachmentContentByType(deserialized, "stream") as MemoryStream).ToArray();
            var buffer1 = (GetAttachmentContentByType(deserialized, "stream/empty") as MemoryStream).ToArray();
            var buffer2 = ((GetAttachmentContentByType(deserialized, "stream/dict") as Dictionary<string, object>)["stream"] as MemoryStream).ToArray();
            var buffer3 = (((GetAttachmentContentByType(deserialized, "stream/dict/nested") as Dictionary<string, object>)["stream"] as Dictionary<string, object>)["content"] as MemoryStream).ToArray();
            var buffer4 = ((GetAttachmentContentByType(deserialized, "stream/list") as List<object>)[0] as MemoryStream).ToArray();
            var buffer4_1 = ((GetAttachmentContentByType(deserialized, "stream/list") as List<object>)[1] as MemoryStream).ToArray();
            var buffer5 = (((GetAttachmentContentByType(deserialized, "stream/list/nested") as List<object>)[0] as List<object>)[0] as MemoryStream).ToArray();

            Assert.Equal(text, Encoding.UTF8.GetString(buffer0));
            Assert.Equal(buffer, buffer0);
            Assert.Equal([], buffer1);
            Assert.Equal(buffer, buffer2);
            Assert.Equal(buffer, buffer3);
            Assert.Equal(buffer, buffer4);
            Assert.Equal(buffer, buffer4_1);
            Assert.Equal(buffer, buffer5);
        }

        [Fact]
        public void MemoryStreamAttachmentShouldFailWithoutJsonConverter()
        {
            var text = "Hi!";
            var buffer = Encoding.UTF8.GetBytes(text);
            var activity = new ActivityDummy
            {
                Attachments = new Attachment[]
                {
                    new AttachmentDummy { ContentType = "stream", Content = new MemoryStream(buffer) },
                }
            };

            var ex = Assert.Throws<JsonSerializationException>(() => JsonConvert.SerializeObject(activity, new JsonSerializerSettings { MaxDepth = null }));
            Assert.Contains("ReadTimeout", ex.Message);
        }

        private void AssertAttachment<T>(T activity)
            where T : Activity
        {
            var serialized = JsonConvert.SerializeObject(activity, new JsonSerializerSettings { MaxDepth = null });
            var deserialized = JsonConvert.DeserializeObject<T>(serialized);

            var attachment0 = GetAttachmentContentByType(deserialized, "string") as string;
            var attachment1 = (GetAttachmentContentByType(deserialized, "string/array") as JArray).First.Value<string>();
            var attachment2 = GetAttachmentContentByType(deserialized, "dict") as JObject;
            var attachment3 = (GetAttachmentContentByType(deserialized, "attachment") as JObject).Value<string>("content");
            var attachment4 = (GetAttachmentContentByType(deserialized, "attachment/dict") as JObject).GetValue("attachment").Value<string>("content");
            var attachment5 = ((GetAttachmentContentByType(deserialized, "attachment/dict/nested") as JObject).GetValue("attachment") as JObject).GetValue("content").Value<string>("content");
            var attachment6 = (GetAttachmentContentByType(deserialized, "attachment/list") as JArray)[0].Value<string>("content");
            var attachment6_1 = (GetAttachmentContentByType(deserialized, "attachment/list") as JArray)[1].Value<string>("content");
            var attachment7 = (GetAttachmentContentByType(deserialized, "attachment/list/nested") as JArray).First.First.Value<string>("content");

            var expectedString = GetAttachmentContentByType(activity, "string") as string;
            var expectedDict = GetAttachmentContentByType(activity, "dict") as Dictionary<string, object>;
            Assert.Equal(expectedString, attachment0);
            Assert.Equal(expectedString, attachment1);
            Assert.Equal($"{expectedDict["firstname"]} {expectedDict["lastname"]} {expectedDict["age"]}", $"{attachment2["firstname"]} {attachment2["lastname"]} {attachment2["age"]}");
            Assert.Equal(expectedString, attachment3);
            Assert.Equal(expectedString, attachment4);
            Assert.Equal(expectedString, attachment5);
            Assert.Equal(expectedString, attachment6);
            Assert.Equal(expectedString, attachment6_1);
            Assert.Equal(expectedString, attachment7);
        }

        private object GetAttachmentContentByType<T>(T activity, string contenttype)
            where T : Activity
        {
            var attachment = activity.Attachments.First(e => e.ContentType == contenttype);
            return attachment.Content ?? (attachment as AttachmentDummy).Content;
        }

        public class ActivityDummy : Activity
        {
        }

        public class AttachmentDummy : Attachment
        {
            public AttachmentDummy(object content = default)
            {
                Content = content;
            }

            [JsonProperty(PropertyName = "content")]
            public new object Content { get; set; }
        }
    }
}
