// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Debugging.CodeModels;
using Microsoft.Bot.Builder.Dialogs.Debugging.Transport;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests
{
    public sealed class DialogDebugAdapterTests
    {
        [Fact]
        public void DialogDebugAdapter_Constructor_Default_Values()
        {
            var codeModel = new CodeModel();
            IBreakpoints breakpoints = new DebuggerSourceMap(codeModel);

            var adapter = new DialogDebugAdapter(
                new DebugTransport(3983, null),
                DebugSupport.SourceMap,
                breakpoints,
                null,
                null,
                null,
                null,
                null);
            
            Assert.NotNull(adapter);
        }

        [Fact]
        public void DialogDebugAdapter_Constructor_Null_Transport_Throws()
        {
            var codeModel = new CodeModel();
            IBreakpoints breakpoints = new DebuggerSourceMap(codeModel);

            Assert.Throws<ArgumentNullException>(() =>
            {
                var adapter = new DialogDebugAdapter(
                null,
                DebugSupport.SourceMap,
                breakpoints,
                null,
                null,
                null,
                null,
                null);
            });
        }

        [Fact]
        public void DialogDebugAdapter_Constructor_Null_sourceMap_Throws()
        {
            var codeModel = new CodeModel();
            IBreakpoints breakpoints = new DebuggerSourceMap(codeModel);

            Assert.Throws<ArgumentNullException>(() =>
            {
                var adapter = new DialogDebugAdapter(
                    new DebugTransport(3984, null),
                    null,
                    breakpoints,
                    null,
                    null,
                    null,
                    null,
                    null);
            });
        }

        [Fact]
        public void DialogDebugAdapter_Constructor_Null_BreakPoints_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var adapter = new DialogDebugAdapter(
                    new DebugTransport(3985, null),
                    DebugSupport.SourceMap,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null);
            });
        }
    }
}
