// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.LanguageGeneration.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class SourceMapTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
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
                Assert.IsTrue(sourceMap.TryGetValue(item, out range), "couldn't find item");
                Assert.AreEqual($"{item.Index}.txt", range.Path);
                Assert.AreEqual(10 + item.Index, range.StartPoint.LineIndex);
                Assert.AreEqual(20 + item.Index, range.StartPoint.CharIndex);
                Assert.AreEqual(30 + item.Index, range.EndPoint.LineIndex);
                Assert.AreEqual(40 + item.Index, range.EndPoint.CharIndex);
            }

            Assert.IsFalse(sourceMap.TryGetValue(new object(), out range), "shouldn't find item");
        }

        [TestMethod]
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
                Assert.IsFalse(sourceMap.TryGetValue(item, out range), "shouldn't find item");
            }
        }

        public class Item
        {
            public int Index { get; set; }
        }
    }
}
