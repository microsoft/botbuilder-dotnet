// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("State")]
    public class RootFrameTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_Should_Throw_On_NullFrameDefinition()
        {
            var x = new RootFrame(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Ctor_Should_Throw_On_Empty_Definition_Namespace()
        {            
            var fd = new FrameDefinition();
            var x = new RootFrame(fd);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Ctor_Should_Throw_On_Empty_Definition_Scope()
        {            
            var fd = new FrameDefinition()
            {
                NameSpace = "testNamespace",
            };

            var x = new RootFrame(fd);
        }

        [TestMethod]
        public void Ctor_Should_Not_Throw()
        {
            var fd = new FrameDefinition()
            {
                NameSpace = "testNamespace",
                Scope = "testScope"
            };

            var x = new RootFrame(fd);
        }

        [TestMethod]
        public void Parent_Should_Be_Null()
        {            
            var x = CreateRootFrame(); 

            // The "Parent" of the root frame is always null. 
            Assert.IsNull(x.Parent);
        }  

        [TestMethod]
        public void Root_Should_Pickup_Scope_And_Namespace_From_Definition()
        {            
            var root = CreateRootFrame("testNamespace", "testScope");

            Assert.IsTrue(root.Namespace == "testNamespace");
            Assert.IsTrue(root.Scope == "testScope");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddSlot_Should_Throw_On_Null()
        {
            var root = CreateRootFrame();
            root.AddSlot(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddSlot_Frame_Mismatch_Should_Throw()
        {
            var root = CreateRootFrame();
            var differentRoot = CreateRootFrame();

            var sd = new SlotDefinition()
            {
                Name = "testName",
            };

            var slot = new Slot(root, sd);

            // Given the rules in AddSlot, this should throw
            // as the Slot is "bound" to a different Root Frame. 
            differentRoot.AddSlot(slot); 
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddSlot_Duplicate_Name_Should_Throw()
        {
            var root = CreateRootFrame();            

            var sd = new SlotDefinition()
            {
                Name = "testName",
            };

            var differentButSameName = new SlotDefinition()
            {
                Name = "testName",
            };

            var slot = new Slot(root, sd);
            var differentSlotButSameName = new Slot(root, differentButSameName);

            root.AddSlot(slot);

            // Given the rules in AddSlot, this should throw
            // as the Slot is "bound" to a different Root Frame. 
            root.AddSlot(differentSlotButSameName);
        }

        [TestMethod]        
        public void AddSlot_And_GetSlot_Should_Work()
        {
            var sd = new SlotDefinition()
            {
                Name = "testName",
            };

            var root = CreateRootFrame();
            var slot = new Slot(root, sd);

            // Note: The slot ctor already called AddSlot. 
            // root.AddSlot(slot);

            // This will throw if the slot is not present. 
            var resultSlot = root.GetSlot("testName");

            // Double check null behavior hasn't crept in.
            Assert.IsNotNull(resultSlot);            
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSlot_Should_Throw_On_Null()
        {
            var root = CreateRootFrame();
            var resultSlot = root.GetSlot(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetSlot_Should_Throw_On_Unknown_Key()
        {
            var root = CreateRootFrame();
            root.GetSlot("unknown");
        }

        private static RootFrame CreateRootFrame(string ns = "testNamespace", string scope = "testScope")
        {
            var fd = new FrameDefinition()
            {
                NameSpace = ns,
                Scope = "testScope",
            };

            return new RootFrame(fd);
        }
    }
}
