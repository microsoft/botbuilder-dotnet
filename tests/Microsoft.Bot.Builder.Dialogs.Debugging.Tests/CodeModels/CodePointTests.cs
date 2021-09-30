// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Debugging.CodeModels;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests.CodeModels
{
    public sealed class CodePointTests
    {
        [Fact]
        public void CodePoint_Constructor()
        {
            ICodeModel codeModel = new CodeModel();
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(new TestAdapter(), activity);
            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            ICodePoint codePoint = new CodePoint(codeModel, dc, "test-item", "more");

            Assert.Equal("test-item", codePoint.Item);
            Assert.Equal("more", codePoint.More);
            Assert.NotNull(codePoint.Data);
        }

        [Fact]
        public void CodePoint_ToString()
        {
            ICodeModel codeModel = new CodeModel();
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(new TestAdapter(), activity);
            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            ICodePoint codePoint = new CodePoint(codeModel, dc, "test-item", "more");

            Assert.Equal(codePoint.ToString(), codePoint.Name);
        }

        [Fact]
        public void CodePoint_Evaluate_Invalid_Expression_Throws()
        {
            ICodeModel codeModel = new CodeModel();
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(new TestAdapter(), activity);
            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            ICodePoint codePoint = new CodePoint(codeModel, dc, "test-item", "more");

            Assert.Throws<InvalidOperationException>(() =>
            {
                codePoint.Evaluate("test-expression");
            });
        }

        [Fact]
        public void CodePoint_Evaluate()
        {
            ICodeModel codeModel = new CodeModel();
            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(new TestAdapter(), activity);
            var dc = new DialogContext(new DialogSet(), context, new DialogState());
            ICodePoint codePoint = new CodePoint(codeModel, dc, "test-item", "more");

            var result = codePoint.Evaluate("123");
            
            Assert.Equal(123, result);
        }
    }
}
