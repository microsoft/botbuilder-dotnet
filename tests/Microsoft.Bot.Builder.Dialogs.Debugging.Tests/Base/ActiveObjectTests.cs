// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Debugging.Base;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests.Base
{
    public sealed class ActiveObjectTests
    {
        [Fact]
        public void ActiveObject_NullInvokeAsync_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ActiveObject(null));
        }
    }
}
