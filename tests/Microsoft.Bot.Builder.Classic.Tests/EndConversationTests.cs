// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public sealed class EndConversationTests : DialogTestBase
    {
        [Serializable]
        private sealed class TestResetDialog : IDialog<object>
        {
            async Task IDialog<object>.StartAsync(IDialogContext context)
            {
                context.Wait(MessageReceivedAsync);
            }

            public static int Increment(IBotDataBag bag, int start)
            {
                const string Key = "key";
                int value;
                if (bag.TryGetValue(Key, out value))
                {
                    value = value + 1;
                }
                else
                {
                    value = start;
                }

                bag.SetValue(Key, value);

                return value;
            }

            public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
            {
                var message = await item;
                if (message.Text == "reset")
                {
                    context.EndConversation("end of conversation");
                }
                else
                {
                    var v1 = Increment(context.PrivateConversationData, 1);
                    var v2 = Increment(context.ConversationData, 2);
                    var v3 = Increment(context.UserData, 3);

                    await context.PostAsync($"echo {message.Text} {v1} {v2} {v3}");
                    context.Wait(MessageReceivedAsync);
                }
            }
        }

        [TestMethod]
        public async Task EndConversation_Resets_Data()
        {
            var dialog = new TestResetDialog();
            using (var container = Build(Options.ResolveDialogFromContainer))
            using (var scope = container.BeginLifetimeScope(
                builder => builder.RegisterInstance(dialog).As<IDialog<object>>()))
            {
                await AssertScriptAsync(scope,
                    "hello",
                    "echo hello 1 2 3",
                    "world",
                    "echo world 2 3 4",
                    "reset",
                    "end of conversation",
                    "hello",
                    "echo hello 1 2 5",
                    "world",
                    "echo world 2 3 6"
                    );
            }
        }
    }
}