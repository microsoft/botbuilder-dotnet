// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Xunit;
using CollectionExtensions = Microsoft.Bot.Builder.Integration.Runtime.Extensions.CollectionExtensions;

namespace Microsoft.Bot.Builder.Runtime.Tests.Extensions
{
    public class CollectionExtensionTests
    {
        public static IEnumerable<object[]> GetAddRangeArgumentNullExceptionData()
        {
            yield return new object[]
            {
                "collection",
                (ICollection<int>)null,
                (IEnumerable<int>)Array.Empty<int>()
            };

            yield return new object[]
            {
                "items",
                (ICollection<int>)Array.Empty<int>(),
                (IEnumerable<int>)null
            };
        }

        public static IEnumerable<object[]> GetAddRangeSucceedsData()
        {
            yield return new object[]
            {
                (ICollection<int>)new List<int> { 1 },
                (IEnumerable<int>)new int[] { 2 }
            };

            yield return new object[]
            {
                (ICollection<int>)new HashSet<int> { 1 },
                (IEnumerable<int>)new int[] { 2 }
            };

            yield return new object[]
            {
                (ICollection<int>)new LinkedList<int>(new[] { 1 }),
                (IEnumerable<int>)new int[] { 2 }
            };
        }

        [Theory]
        [MemberData(nameof(GetAddRangeArgumentNullExceptionData))]
        public void AddRange_Throws_ArgumentNullException(
            string paramName,
            ICollection<int> collection,
            IEnumerable<int> items)
        {
            Assert.Throws<ArgumentNullException>(
                paramName,
                () => CollectionExtensions.AddRange(collection, items));
        }

        [Theory]
        [MemberData(nameof(GetAddRangeSucceedsData))]
        public void AddRange_Succeeds(
            ICollection<int> collection,
            IEnumerable<int> items)
        {
            var expected = new List<int>(collection);
            foreach (int item in items)
            {
                expected.Add(item);
            }

            CollectionExtensions.AddRange(collection, items);

            Assert.Equal((ICollection<int>)expected, collection);
        }
    }
}
