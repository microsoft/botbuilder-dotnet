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
using Microsoft.Bot.Builder.Classic.Scorables;
using Microsoft.Bot.Builder.Classic.Scorables.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public sealed class ActionsTests : ScorableTestBase
    {
        public static readonly string Item = "hello";
        public static readonly double Score = 1.0;
        public static readonly CancellationToken Token = new CancellationTokenSource().Token;

        [TestMethod]
        public async Task Scorable_Where()
        {
            foreach (var hasScore in new[] { false, true })
            {
                foreach (var whereScore in new[] { false, true })
                {
                    // arrange
                    IResolver resolver = new ArrayResolver(NullResolver.Instance, Item);
                    var mock = Mock(resolver, Score, Token, hasScore);
                    var test = mock.Object.Where((string item, double score) =>
                    {
                        Assert.AreEqual(Item, item);
                        Assert.AreEqual(Score, score);
                        return whereScore;
                    });
                    // act
                    bool actualPost = await test.TryPostAsync(resolver, Token);
                    // assert
                    bool expectedPost = hasScore && whereScore;
                    Assert.AreEqual(expectedPost, actualPost);
                    Verify(mock, resolver, Token, Once(true), Once(true), Many(hasScore), Once(expectedPost));
                }
            }
        }

        [TestMethod]
        public async Task Scorable_Where_Task()
        {
            foreach (var hasScore in new[] { false, true })
            {
                foreach (var whereScore in new[] { false, true })
                {
                    // arrange
                    IResolver resolver = new ArrayResolver(NullResolver.Instance, Item);
                    var mock = Mock(resolver, Score, Token, hasScore);
                    var test = mock.Object.Where(async (string item, double score) =>
                    {
                        Assert.AreEqual(Item, item);
                        Assert.AreEqual(Score, score);
                        return whereScore;
                    });
                    // act
                    bool actualPost = await test.TryPostAsync(resolver, Token);
                    // assert
                    bool expectedPost = hasScore && whereScore;
                    Assert.AreEqual(expectedPost, actualPost);
                    Verify(mock, resolver, Token, Once(true), Once(true), Many(hasScore), Once(expectedPost));
                }
            }
        }
    }
}
