// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Debugging.CodeModels;
using Microsoft.Bot.Connector.Authentication;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests
{
    public sealed class DebuggingBotAdapterExtensionsTests
    {
        [Fact]
        public void DebuggingBotAdapterExtensions_UseDebugger()
        {
            var codeModel = new CodeModel();
            ISourceMap sourceMap = new DebuggerSourceMap(codeModel);

            var adapter = new BotFrameworkAdapter(null, new SimpleCredentialProvider());

            Assert.Single(adapter.MiddlewareSet);
            
            adapter.UseDebugger(3982, sourceMap);

            Assert.Equal(2, adapter.MiddlewareSet.Count());
        }
    }
}
