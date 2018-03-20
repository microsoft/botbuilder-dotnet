// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public sealed class RangeTests
    {
        public static IEnumerable<IEnumerable<T>> Subsets<T>(IEnumerable<T> items) where T: IEquatable<T>
        {
            yield return items;

            foreach (var item in items)
            {
                var without = items.Where(i => ! i.Equals(item)).ToArray();
                if (without.Length > 0)
                {
                    foreach (var subset in Subsets(without))
                    {
                        yield return subset;
                    }
                }
            }
        }

        public static IEnumerable<Range<int>> ToRanges(IEnumerable<int> items)
        {
            return items
                .OrderBy(i => i)
                .Select(i => new[] { Range.From(i, i + 1) })
                .Aggregate((l, r) =>
                {
                    var one = l[l.Length - 1];
                    var two = r[0];
                    if (one.After == two.Start)
                    {
                        return l
                        .Take(l.Length - 1)
                        .Concat(new[] { Range.From(one.Start, two.After) })
                        .Concat(r.Skip(1))
                        .ToArray();
                    }
                    else
                    {
                        return l.Concat(r).ToArray();
                    }
                })
                .ToArray();
        }

        [TestMethod]
        public void Range_SortedMerge()
        {
            var items = Enumerable.Range(0, 5);
            var subsets = Subsets(items).ToArray();
            var ranges = subsets.Select(ToRanges).ToArray();

            foreach (var one in ranges)
            {
                Func<IEnumerable<Range<int>>, int[]> Flatten = source => source.SelectMany(i => i.Enumerate()).ToArray();

                var oneItems = Flatten(one);
                foreach (var two in ranges)
                {
                    var twoItems = Flatten(two);
                    var actual = Flatten(one.SortedMerge(two));
                    var expected = oneItems.Intersect(twoItems).OrderBy(i => i).ToArray();
                    CollectionAssert.AreEqual(expected, actual);
                }
            }
        }
    }
}
