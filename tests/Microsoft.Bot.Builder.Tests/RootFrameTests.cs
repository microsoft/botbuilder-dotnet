// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("State")]
    public class RootFrameTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_Should_Throw_On_NullStorage()
        {
            var x = new RootFrame(null, new FrameDefinition());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_Should_Throw_On_NullFrameDefinition()
        {
            var m = new MemoryStorage();
            var x = new RootFrame(m, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Ctor_Should_Throw_On_Empty_Definition_Namespace()
        {
            var m = new MemoryStorage();
            var fd = new FrameDefinition();
            var x = new RootFrame(m, fd);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Ctor_Should_Throw_On_Empty_Definition_Ccope()
        {
            var m = new MemoryStorage();
            var fd = new FrameDefinition()
            {
                NameSpace = "testNamespace",
            };

            var x = new RootFrame(m, fd);
        }

        [TestMethod]
        public void Ctor_Should_Not_Throw()
        {
            var m = new MemoryStorage();
            var fd = new FrameDefinition()
            {
                NameSpace = "testNamespace",
                Scope = "testScope"
            };

            var x = new RootFrame(m, fd);
        }

        [TestMethod]
        public void Parent_Should_Be_Null()
        {
            var m = new MemoryStorage();
            var fd = new FrameDefinition()
            {
                NameSpace = "testNamespace",
                Scope = "testScope",
            };

            var x = new RootFrame(m, fd);

            // The "Parent" of the root frame is always null. 
            Assert.IsNull(x.Parent);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Parent_Should_Not_Be_Settable()
        {
            var fd = new FrameDefinition()
            {
                NameSpace = "testNamespace",
                Scope = "testScope",
            };

            var root = new RootFrame(new Mock<IStorage>().Object, fd);

            var mockFrame = new Mock<IFrame>();

            // Should throw an exception, as the
            // parent on the root frame is not settable. 
            root.Parent = mockFrame.Object;
        }

        [TestMethod]
        public void Parent_Should_Pickup_Scope_And_Namespace_From_Definition()
        {            
            var fd = new FrameDefinition()
            {
                NameSpace = "testNamespace",
                Scope = "testScope",
            };

            var root = new RootFrame(new Mock<IStorage>().Object, fd);

            Assert.IsTrue(root.Namespace == "testNamespace");
            Assert.IsTrue(root.Scope == "testScope");
        }
    }
}
