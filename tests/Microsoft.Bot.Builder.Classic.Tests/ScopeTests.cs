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
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public sealed class ScopeTests
    {
        public const string KeyOne = "one";
        public const string KeyTwo = "two";

        [TestMethod]
        public async Task LocalMutualExclusion_Serializes()
        {
            IScope<string> mutex = new LocalMutualExclusion<string>(EqualityComparer<string>.Default);

            var taskOne = mutex.WithScopeAsync(KeyOne, CancellationToken.None);
            var taskTwo = mutex.WithScopeAsync(KeyTwo, CancellationToken.None);

            var taskWaiting = mutex.WithScopeAsync(KeyOne, CancellationToken.None);

            using (await taskOne)
            using (await taskTwo)
            {
                Assert.IsFalse(taskWaiting.IsCompleted);
            }

            using (await taskWaiting)
            {
            }
        }

        [TestMethod]
        public async Task LocalMutualExclusion_NoDeadlock()
        {
            IScope<string> mutex = new LocalMutualExclusion<string>(EqualityComparer<string>.Default);

            using (await mutex.WithScopeAsync(KeyOne, CancellationToken.None))
            using (await mutex.WithScopeAsync(KeyTwo, CancellationToken.None))
            {
            }

            using (await mutex.WithScopeAsync(KeyTwo, CancellationToken.None))
            using (await mutex.WithScopeAsync(KeyOne, CancellationToken.None))
            {
            }

            using (await mutex.WithScopeAsync(KeyOne, CancellationToken.None))
            using (await mutex.WithScopeAsync(KeyTwo, CancellationToken.None))
            {
            }
        }

        [TestMethod]
        public async Task LocalMutualExclusion_ReferenceCounts()
        {
            var local = new LocalMutualExclusion<string>(EqualityComparer<string>.Default);
            IScope<string> mutex = local;

            Action<string, int> AssertReferenceCount = (key, expected) =>
            {
                int actual;
                Assert.IsTrue(local.TryGetReferenceCount(key, out actual));
                Assert.AreEqual(expected, actual);
            };

            var taskA = mutex.WithScopeAsync(KeyOne, CancellationToken.None);
            AssertReferenceCount(KeyOne, 1);

            var taskB = mutex.WithScopeAsync(KeyOne, CancellationToken.None);
            AssertReferenceCount(KeyOne, 2);

            using (await taskA)
            {
                AssertReferenceCount(KeyOne, 2);
                Assert.IsFalse(taskB.IsCompleted);
            }

            AssertReferenceCount(KeyOne, 1);

            using (await taskB)
            {
                AssertReferenceCount(KeyOne, 1);
            }

            int zero;
            Assert.IsFalse(local.TryGetReferenceCount(KeyOne, out zero), "did not clean up per-item semaphore resource");
        }
    }
}
