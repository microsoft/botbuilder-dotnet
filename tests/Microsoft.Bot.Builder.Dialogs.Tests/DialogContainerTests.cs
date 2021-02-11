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
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class DialogContainerTests
    {
        [Fact]
        public void DialogContainer_GetVersion()
        {
            var ds = new TestContainer();
            var version1 = ds.GetInternalVersion_Test();
            Assert.NotNull(version1);

            var ds2 = new TestContainer();
            var version2 = ds.GetInternalVersion_Test();
            Assert.NotNull(version2);
            Assert.Equal(version1, version2);

            ds2.Dialogs.Add(new LamdaDialog((dc, ct) => null) { Id = "A" });
            var version3 = ds2.GetInternalVersion_Test();
            Assert.NotNull(version3);
            Assert.NotEqual(version2, version3);

            var version4 = ds2.GetInternalVersion_Test();
            Assert.NotNull(version3);
            Assert.Equal(version3, version4);

            var ds3 = new TestContainer();
            ds3.Dialogs.Add(new LamdaDialog((dc, ct) => null) { Id = "A" });

            var version5 = ds3.GetInternalVersion_Test();
            Assert.NotNull(version5);
            Assert.Equal(version5, version4);

            ds3.Property = "foobar";
            var version6 = ds3.GetInternalVersion_Test();
            Assert.NotNull(version6);
            Assert.NotEqual(version6, version5);

            var ds4 = new TestContainer()
            {
                Property = "foobar"
            };

            ds4.Dialogs.Add(new LamdaDialog((dc, ct) => null) { Id = "A" });
            var version7 = ds4.GetInternalVersion_Test();
            Assert.NotNull(version7);
            Assert.Equal(version7, version6);
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
