﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Tests;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [TestClass]
    public class ValueRecognizerTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(ValueRecognizerTests)), monitorChanges: false);
        }

        [TestMethod]
        public async Task ValueRecognizerTests_WithIntent()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task ValueRecognizerTests_WithNoIntent()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }
    }
}
