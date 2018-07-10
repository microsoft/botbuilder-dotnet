// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("State")]
    [TestCategory("POCO")]
    public class FrameDefinitionTests
    {
        [TestMethod]
        public void CreateAndSet()
        {
            const string targetScope = "testScope";
            const string targetNs = "testNamespace";

            var fd = new FrameDefinition()
            {
                Scope = targetScope,
                NameSpace = targetScope,
            };

            Assert.AreEqual(targetScope, fd.Scope);
            Assert.AreEqual(targetNs, fd.NameSpace);

            // Should defult to empty. 
            Assert.IsTrue(fd.SlotDefinitions.Count == 0);
        }
    }
}
