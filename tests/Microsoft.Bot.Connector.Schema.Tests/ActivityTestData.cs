// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

namespace Microsoft.Bot.Connector.Schema.Tests
{
    internal class ActivityTestData
    {
        internal class TestChannelData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { new Dictionary<string, JsonElement>() };
                yield return new object[] { null };
                yield return new object[] { new Dictionary<string, string>() };
                yield return new object[] { new MyChannelData() };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal class HasContentData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { new Activity() { Text = "text" }, true };
                yield return new object[] { new Activity() { Summary = "summary" }, true };
                yield return new object[] { new Activity() { Attachments = GetAttachments() }, true };
                yield return new object[] { new Activity() { ChannelData = new MyChannelData() }, true };
                yield return new object[] { new Activity(), false };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private IList<Attachment> GetAttachments()
            {
                return new List<Attachment> { new Attachment() };
            }
        }

        internal class MentionsData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    new List<Entity> { new Entity() },
                    false
                };

                var mention = new
                {
                    Mentioned = new
                    {
                        Id = "ChannelAccountId",
                        Name = "AccountName",
                        Properties = new Dictionary<string, JsonElement>(),
                        Role = "ChannelAccountRole"
                    },
                    Text = "text",
                    Type = "mention"
                };

                yield return new object[]
                {
                    new List<Entity> { new Entity { Type = "mention", Properties = mention.ToJsonElements() } },
                    true
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal class MyChannelData
        {
            public string Ears { get; set; }

            public string Whiskers { get; set; }
        }

        internal class TestActivity : Activity
        {
            public bool IsTargetActivityType(string activityType)
            {
                return IsActivity(activityType);
            }
        }
    }
}
