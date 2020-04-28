// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable SA1402 // File may only contain a single type

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    public class DialogContainerTests
    {
        [TestMethod]
        public void DialogContainer_GetVersion()
        {
            var ds = new TestContainer();
            var version1 = ds.GetInternalVersion_Test();
            Assert.IsNotNull(version1);

            var ds2 = new TestContainer();
            var version2 = ds.GetInternalVersion_Test();
            Assert.IsNotNull(version2);
            Assert.AreEqual(version1, version2, "Same configuration should give same version");

            ds2.Dialogs.Add(new LamdaDialog((dc, ct) => null) { Id = "A" });
            var version3 = ds2.GetInternalVersion_Test();
            Assert.IsNotNull(version3);
            Assert.AreNotEqual(version2, version3, "version should change if there is a dialog added");

            var version4 = ds2.GetInternalVersion_Test();
            Assert.IsNotNull(version3);
            Assert.AreEqual(version3, version4, "version be same if there is no change");

            var ds3 = new TestContainer();
            ds3.Dialogs.Add(new LamdaDialog((dc, ct) => null) { Id = "A" });

            var version5 = ds3.GetInternalVersion_Test();
            Assert.IsNotNull(version5);
            Assert.AreEqual(version5, version4, "version be same if there is no change");

            ds3.Property = "foobar";
            var version6 = ds3.GetInternalVersion_Test();
            Assert.IsNotNull(version6);
            Assert.AreNotEqual(version6, version5, "version should change if property changes");

            var ds4 = new TestContainer()
            {
                Property = "foobar"
            };

            ds4.Dialogs.Add(new LamdaDialog((dc, ct) => null) { Id = "A" });
            var version7 = ds4.GetInternalVersion_Test();
            Assert.IsNotNull(version7);
            Assert.AreEqual(version7, version6, "version be the same when constructed the same way");
        }
    }

    public class TestContainer : DialogContainer
    {
        public string Property { get; set; }

        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            return dc.EndDialogAsync();
        }

        public override DialogContext CreateChildContext(DialogContext dc)
        {
            return dc;
        }

        public string GetInternalVersion_Test()
        {
            return GetInternalVersion();
        }

        protected override string GetInternalVersion()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInternalVersion());
            sb.Append(Property ?? string.Empty);

            return StringUtils.Hash(sb.ToString());
        }
    }
}
