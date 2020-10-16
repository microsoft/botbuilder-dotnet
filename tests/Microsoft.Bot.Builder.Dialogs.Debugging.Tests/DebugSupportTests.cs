// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests
{
    public class DebugSupportTests : IDisposable
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DebugSupport_HasDifferentInstancesInDifferentContexts_WhenAsyncLocal(bool isAsyncLocal)
        {
            DebugSupport.UseAsyncLocal = isAsyncLocal;

            var sourceMap1 = await GetLocalSourceMapAsync();
            var sourceMap2 = await GetLocalSourceMapAsync();

            bool isSameInstance = object.ReferenceEquals(sourceMap1, sourceMap2);
            if (isSameInstance && isAsyncLocal)
            {
                throw new Exception("Expected a different instance in each AsyncLocal context when AsyncLocal is used.");
            }

            if (!isSameInstance && !isAsyncLocal)
            {
                throw new Exception("Expected a singleton instance when AsyncLocal is not used. ");
            }
        }

        public void Dispose()
        {
            // do not interfere with other tests
            DebugSupport.UseAsyncLocal = false;
        }

        private async Task<ISourceMap> GetLocalSourceMapAsync()
        {
            await Task.Yield();
            return DebugSupport.SourceMap;
        }
    }
}
