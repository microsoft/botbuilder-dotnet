// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Debugging.CodeModels;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests.CodeModels
{
    public sealed class CodeModelTests
    {
        [Fact]
        public void CodeModel_NameFor_String_Item()
        {
            ICodeModel codeModel = new CodeModel();

            var returnedValue = codeModel.NameFor("item");

            Assert.Equal("String", returnedValue);
        }

        [Fact]
        public void CodeModel_NameFor_Dialog_Item()
        {
            ICodeModel codeModel = new CodeModel();
            var item = new WaterfallDialog("dialog-id");

            var returnedValue = codeModel.NameFor(item);

            Assert.Equal(item.Id, returnedValue);
        }

        [Fact]
        public void CodeModel_NameFor_Identity_Item()
        {
            ICodeModel codeModel = new CodeModel();
            IItemIdentity item = new OnMessageActivity();

            var returnedValue = codeModel.NameFor(item);

            Assert.Equal("OnMessageActivity[]", returnedValue);
        }

        [Fact]
        public void CodeModel_PointsFor()
        {
            ICodeModel codeModel = new CodeModel();
            var item = new WaterfallDialog("dialog-id");

            var activity = MessageFactory.Text("hi");
            var context = new TurnContext(new TestAdapter(), activity);

            var dialogs = new DialogSet();
            dialogs.Add(item);

            var dc = new DialogContext(dialogs, context, new DialogState());

            var instance = new DialogInstance { Id = "dialog-id" };

            dc.Stack.Add(instance);

            var codePoints = codeModel.PointsFor(dc, item, "more");

            Assert.Equal(item, codePoints[0].Item);
        }
    }
}
