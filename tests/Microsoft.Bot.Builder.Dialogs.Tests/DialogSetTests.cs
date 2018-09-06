// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    [TestClass]
    class DialogSetTests
    {

        [TestMethod]
        public void DialogSet_ConstructorValid()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var ds = new DialogSet(dialogStateProperty);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DialogSet_ConstructorNullProperty()
        {
            var ds = new DialogSet(null);
        }

        [TestMethod]
        public async Task DialogSet_CreateContextAsync()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var ds = new DialogSet(dialogStateProperty);
            var context = TestUtilities.CreateEmptyContext();
            var dc = await ds.CreateContextAsync(context);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DialogSet_NullCreateContextAsync()
        {
            var convoState = new ConversationState(new MemoryStorage());
            var dialogStateProperty = convoState.CreateProperty<DialogState>("dialogstate");
            var ds = new DialogSet(dialogStateProperty);
            var context = TestUtilities.CreateEmptyContext();
            var dc = await ds.CreateContextAsync(context);
        }

    }
}
