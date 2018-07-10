// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

    }
}
