// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class SourceMapTests
    {
        [Fact]
        public void TestSourceMap()
        {
            ISourceMap sourceMap = new SourceMap();
            List<Item> items = new List<Item>();
            for (int i = 0; i < 5; i++)
            {
                var item = new Item() { Index = i };
                sourceMap.Add(item, new SourceRange()
                {
                    Path = $"{i}.txt",
                    StartPoint = new SourcePoint(lineIndex: 10 + i, charIndex: 20 + i),
                    EndPoint = new SourcePoint() { LineIndex = 30 + i, CharIndex = 40 + i }
                });
                items.Add(item);
            }

            SourceRange range;
            foreach (var item in items)
            {
                Assert.True(sourceMap.TryGetValue(item, out range), "couldn't find item");
                Assert.Equal($"{item.Index}.txt", range.Path);
                Assert.Equal(10 + item.Index, range.StartPoint.LineIndex);
                Assert.Equal(20 + item.Index, range.StartPoint.CharIndex);
                Assert.Equal(30 + item.Index, range.EndPoint.LineIndex);
                Assert.Equal(40 + item.Index, range.EndPoint.CharIndex);
            }

            Assert.False(sourceMap.TryGetValue(new object(), out range), "shouldn't find item");
        }

        [Fact]
        public void TestNullSourceMap()
        {
            ISourceMap sourceMap = new NullSourceMap();
            List<Item> items = new List<Item>();
            for (int i = 0; i < 5; i++)
            {
                var item = new Item() { Index = i };
                sourceMap.Add(item, new SourceRange()
                {
                    Path = $"{i}.txt",
                    StartPoint = new SourcePoint() { LineIndex = 10 + i, CharIndex = 20 + i },
                    EndPoint = new SourcePoint() { LineIndex = 30 + i, CharIndex = 40 + i }
                });
                items.Add(item);
            }

            SourceRange range;
            foreach (var item in items)
            {
                Assert.False(sourceMap.TryGetValue(item, out range), "shouldn't find item");
            }
        }

        public class Item
        {
            public int Index { get; set; }
        }
    }
}
