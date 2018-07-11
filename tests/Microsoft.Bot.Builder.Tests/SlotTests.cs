// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("State")]
    public class SlotTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_Should_Throw_On_Null_Frame()
        {
            var sd = new SlotDefinition();
            var x = new Slot(null, sd); 
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_Should_Throw_On_Null_SlotDefinition()
        {
            var mockFrame = new Mock<IFrame>();            
            var x = new Slot(mockFrame.Object, null);
        }

        [TestMethod]
        public void Ctor_Should_Not_Throw()
        {
            var mockFrame = new Mock<IFrame>();
            var sd = new SlotDefinition();
            var x = new Slot(mockFrame.Object, sd);
        }

        [TestMethod]
        public void Ctor_Should_Register_The_Frame()
        {
            var mockFrame = new Mock<IFrame>();
            var sd = new SlotDefinition();
            var x = new Slot(mockFrame.Object, sd);                 
            mockFrame.Verify(t => t.AddSlot(x));
        }

        [TestMethod]
        public void Ctor_Should_Register_SlotDefinition()
        {
            var mockFrame = new Mock<IFrame>();                        
            var sd = new SlotDefinition()
            {
                Name = "testName",
            };

            var x = new Slot(mockFrame.Object, sd);
            Assert.IsTrue(x.Definition.Name == "testName");            
        }
    }
}
