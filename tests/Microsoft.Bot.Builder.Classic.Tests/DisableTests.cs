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

using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.History;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public sealed class DisableTests : DialogTestBase
    {
        //[TestMethod]
        //public async Task Disable_PostUnhandledExceptionToUser()
        //{
        //    var dialog = Chain.PostToChain().Do((c, a) =>
        //    {
        //        throw new Exception("hello");
        //    });

        //    using (var container = Build(Options.ResolveDialogFromContainer))
        //    {
        //        {
        //            var builder = new ContainerBuilder();
        //            builder
        //                .RegisterInstance(dialog)
        //                .As<IDialog<object>>();
        //            builder.Update(container);
        //        }

        //        var toBotText = "hello";
        //        var toUsertText = Debugger.IsAttached ? $"Exception: {toBotText}" : "Sorry, my bot code is having an issue.";

        //        var toBot = MakeTestMessage();
        //        toBot.Text = toBotText;

        //        try
        //        {
        //            await PostActivityAsync(container, toBot, CancellationToken.None);
        //        }
        //        catch
        //        {
        //        }

        //        var queue = container.Resolve<Queue<Activity>>();
        //        Assert.AreEqual(1, queue.Count);
        //        Assert.AreEqual(toUsertText, queue.Dequeue().Text);

        //        {
        //            var builder = new ContainerBuilder();
        //            Conversation.Disable(typeof(PostUnhandledExceptionToUser), builder);
        //            builder.Update(container);
        //        }

        //        try
        //        {
        //            await PostActivityAsync(container, toBot, CancellationToken.None);
        //        }
        //        catch
        //        {
        //        }

        //        Assert.AreEqual(0, queue.Count);
        //    }
        //}

        [TestMethod]
        public async Task Disable_LogBotToUser()
        {
            var dialog = Chain.PostToChain().Select(m => m.Text).PostToUser().Loop();

            var mock = new Mock<IActivityLogger>(MockBehavior.Strict);
            mock
                .Setup(l => l.LogAsync(It.IsAny<IActivity>()))
                .Returns(Task.CompletedTask);

            using (var container = Build(Options.ResolveDialogFromContainer))
            using (var containerScope = container.BeginLifetimeScope(
                builder =>
                {
                    builder.RegisterInstance(dialog).As<IDialog<object>>();
                    builder.RegisterInstance(mock.Object).As<IActivityLogger>();
                }))
            {
                var text = "hello";

                await AssertScriptAsync(
                    containerScope,
                    text,
                    text);

                mock.VerifyAll();

                using (var disablingScope = containerScope.BeginLifetimeScope(
                    builder =>
                    {
                        Conversation.Disable(typeof(LogBotToUser), builder);
                        Conversation.Disable(typeof(LogPostToBot), builder);
                    }))
                {
                    mock.Reset();

                    await AssertScriptAsync(
                        disablingScope,
                        text,
                        text);

                    mock.VerifyAll();
                }
            }
        }
    }
}