using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.Tests
{
    internal class ActivityTestData
    {
        internal class TestChannelData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { new JObject() };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal class GetContentData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { "text", null, null, null, true };
                yield return new object[] { null, "summary", null, null, true };
                yield return new object[] { null, null, GetAttachment(), null, true };
                yield return new object[] { null, null, null, new MyChannelData(), true };
                yield return new object[] { null, null, null, null, false };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private IList<Attachment> GetAttachment()
            {
                return new List<Attachment> { new Attachment() };
            }
        }

        internal class MyChannelData
        {
            public string Ears { get; set; }

            public string Whiskers { get; set; }
        }
    }
}
