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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Autofac;
using Microsoft.Bot.Builder.Classic.Scorables.Internals;
using Microsoft.Bot.Schema;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public sealed class ResolverTests
    {
        public interface IService
        {
        }
        public sealed class Service : IService
        {
        }
        public static readonly object Some = new object();

        [TestMethod]
        public void Resolver_Null()
        {
            var resolver = NullResolver.Instance;

            IService service;
            Assert.IsFalse(resolver.TryResolve(null, out service));
            Assert.IsFalse(resolver.TryResolve(Some, out service));
        }

        [TestMethod]
        public void Resolver_None()
        {
            var resolver = NoneResolver.Instance;

            IService service;
            Assert.IsFalse(resolver.TryResolve(null, out service));
            Assert.IsFalse(resolver.TryResolve(Some, out service));

            CancellationToken token;
            Assert.IsTrue(resolver.TryResolve(null, out token));
            Assert.IsFalse(token.CanBeCanceled);
        }

        [TestMethod]
        public void Resolver_Enum()
        {
            var resolver = new EnumResolver(NoneResolver.Instance);

            DayOfWeek actual;
            Assert.IsFalse(resolver.TryResolve(null, out actual));
            Assert.IsFalse(resolver.TryResolve(Some, out actual));

            var expected = DayOfWeek.Monday;
            {
                Assert.IsTrue(resolver.TryResolve(expected.ToString(), out actual));
                Assert.AreEqual(expected, actual);
            }
            {
                Assert.IsFalse(resolver.TryResolve(((int)expected).ToString(), out actual));
                Assert.AreEqual(DayOfWeek.Sunday, actual);
            }
        }

        [TestMethod]
        public void Resolver_Array()
        {
            var expected = new Service();
            var resolver = new ArrayResolver(NullResolver.Instance, expected);

            {
                IService actual;
                Assert.IsTrue(resolver.TryResolve(null, out actual));
                Assert.IsFalse(resolver.TryResolve(Some, out actual));
            }

            {
                Service actual;
                Assert.IsTrue(resolver.TryResolve(null, out actual));
                Assert.IsFalse(resolver.TryResolve(Some, out actual));
            }
        }

        [TestMethod]
        public void Resolver_Activity()
        {
            var expected = new Activity();
            var resolver = new ActivityResolver(new ArrayResolver(NullResolver.Instance, expected));

            expected.Type = ActivityTypes.Message;
            {
                IActivity actual;
                Assert.IsTrue(resolver.TryResolve(null, out actual));
                Assert.IsFalse(resolver.TryResolve(Some, out actual));
            }

            {
                IMessageActivity actual;
                Assert.IsTrue(resolver.TryResolve(null, out actual));
                Assert.IsFalse(resolver.TryResolve(Some, out actual));
            }

            {
                ITypingActivity actual;
                Assert.IsFalse(resolver.TryResolve(null, out actual));
                Assert.IsFalse(resolver.TryResolve(Some, out actual));
            }

            expected.Type = ActivityTypes.Typing;

            {
                ITypingActivity actual;
                Assert.IsTrue(resolver.TryResolve(null, out actual));
                Assert.IsFalse(resolver.TryResolve(Some, out actual));
            }
        }

        [TestMethod]
        public void Resolver_EventActivityValue()
        {
            var expected = new Activity() { Type = ActivityTypes.Event };
            var resolver = new EventActivityValueResolver(new ActivityResolver(new ArrayResolver(NullResolver.Instance, expected)));

            {
                IService actual;
                Assert.IsFalse(resolver.TryResolve(null, out actual));
                Assert.IsFalse(resolver.TryResolve(Some, out actual));
            }

            expected.Value = new Service();

            {
                IService actual;
                Assert.IsTrue(resolver.TryResolve(null, out actual));
                Assert.IsFalse(resolver.TryResolve(Some, out actual));
            }
        }

        [TestMethod]
        public void Resolver_InvokeActivityValue()
        {
            var expected = new Activity() { Type = ActivityTypes.Invoke };
            var resolver = new InvokeActivityValueResolver(new ActivityResolver(new ArrayResolver(NullResolver.Instance, expected)));

            {
                IService actual;
                Assert.IsFalse(resolver.TryResolve(null, out actual));
                Assert.IsFalse(resolver.TryResolve(Some, out actual));
            }

            expected.Value = new Service();

            {
                IService actual;
                Assert.IsTrue(resolver.TryResolve(null, out actual));
                Assert.IsFalse(resolver.TryResolve(Some, out actual));
            }
        }
    }
}
