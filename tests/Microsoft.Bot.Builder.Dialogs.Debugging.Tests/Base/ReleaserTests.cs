// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using Xunit;
using Releaser = Microsoft.Bot.Builder.Dialogs.Debugging.Base.Releaser;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests.Base
{
    public sealed class ReleaserTests
    {
        [Fact]
        public void Releaser_Constructor()
        {
            var semaphore = new SemaphoreSlim(1);
            var releaser = new Releaser(semaphore);

            Assert.Equal(semaphore, releaser.Semaphore);
        }

        [Fact]
        public void Releaser_Constructor_NullSemaphore_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new Releaser(null));
        }

        [Fact]
        public void Releaser_Dispose()
        {
            var semaphore = new SemaphoreSlim(1);
            var releaser = new Releaser(semaphore);

            releaser.Dispose();

            Assert.Equal(2, releaser.Semaphore.CurrentCount);
        }
    }
}
