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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Classic.Base;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.Scorables;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Scorables.Internals;
using Microsoft.Bot.Schema;

using Autofac;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder.Adapters;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public sealed class DialogTaskTests : DialogTestBase
    {
        public interface IDialogThatFails : IDialog<object>
        {
            Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> message);
            Task Throw(IDialogContext context, IAwaitable<IMessageActivity> message);
        }

        [TestMethod]
        public async Task If_Root_Dialog_Throws_Propagate_Exception_Reset_Store()
        {
            var dialog = new Mock<IDialogThatFails>(MockBehavior.Loose);

            dialog
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { context.Wait(dialog.Object.MessageReceived); });

            dialog
                .Setup(d => d.MessageReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()))
                .Returns<IDialogContext, IAwaitable<IMessageActivity>>(async (context, result) => { context.Wait(dialog.Object.Throw); });

            dialog
                .Setup(d => d.Throw(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()))
                .Throws<ApplicationException>();

            Func<IDialog<object>> MakeRoot = () => dialog.Object;
            var toBot = DialogTestBase.MakeTestMessage();

            using (new FiberTestBase.ResolveMoqAssembly(dialog.Object))
            using (var container = Build(Options.MockConnectorFactory, dialog.Object))
            {
                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);

                        var task = scope.Resolve<IPostToBot>();
                        await task.PostAsync(toBot, CancellationToken.None);
                    }
                });

                dialog.Verify(d => d.StartAsync(It.IsAny<IDialogContext>()), Times.Once);
                dialog.Verify(d => d.MessageReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()), Times.Once);
                dialog.Verify(d => d.Throw(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()), Times.Never);

                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);

                        await scope.Resolve<IBotData>().LoadAsync(default(CancellationToken));
                        var task = scope.Resolve<IDialogStack>();
                        Assert.AreNotEqual(0, task.Frames.Count);
                    }
                });

                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);

                        var task = scope.Resolve<IPostToBot>();

                        try
                        {
                            await task.PostAsync(toBot, CancellationToken.None);
                            Assert.Fail();
                        }
                        catch (ApplicationException)
                        {
                        }
                        catch
                        {
                            Assert.Fail();
                        }
                    }
                });

                dialog.Verify(d => d.StartAsync(It.IsAny<IDialogContext>()), Times.Once);
                dialog.Verify(d => d.MessageReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()), Times.Once);
                dialog.Verify(d => d.Throw(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()), Times.Once);

                //make sure that data is persisted with connector
                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        var connectorFactory = scope.Resolve<IConnectorClientFactory>();
                        var botDataStore = scope.Resolve<IBotDataStore<BotData>>();
                        var botData = scope.Resolve<IBotData>();
                        await botData.LoadAsync(default(CancellationToken));
                        // stack + resumption context
                        Assert.AreEqual(2, botData.PrivateConversationData.Count);
                    }
                });

                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);

                        await scope.Resolve<IBotData>().LoadAsync(default(CancellationToken));
                        var stack = scope.Resolve<IDialogStack>();
                        Assert.AreEqual(0, stack.Frames.Count);
                    }
                });

                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);

                        var task = scope.Resolve<IPostToBot>();
                        await task.PostAsync(toBot, CancellationToken.None);
                    }
                });

                dialog.Verify(d => d.StartAsync(It.IsAny<IDialogContext>()), Times.Exactly(2));
                dialog.Verify(d => d.MessageReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()), Times.Exactly(2));
                dialog.Verify(d => d.Throw(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()), Times.Once);
            }
        }

        public interface IDialogFrames<T> : IDialog<T>
        {
            Task ItemReceived<R>(IDialogContext context, IAwaitable<R> item);
        }

        [TestMethod]
        public async Task DialogTask_Frames()
        {
            var dialog = new Mock<IDialogFrames<object>>(MockBehavior.Loose);

            dialog
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { PromptDialog.Text(context, dialog.Object.ItemReceived, "blah"); });

            Func<IDialog<object>> MakeRoot = () => dialog.Object;
            var toBot = MakeTestMessage();

            using (new FiberTestBase.ResolveMoqAssembly(dialog.Object))
            using (var container = Build(Options.None, dialog.Object))
            {
                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);

                        var botData = scope.Resolve<IBotData>();
                        await botData.LoadAsync(default(CancellationToken));

                        var stack = scope.Resolve<IDialogStack>();
                        Assert.AreEqual(0, stack.Frames.Count);

                        var task = scope.Resolve<IPostToBot>();
                        await task.PostAsync(toBot, CancellationToken.None);

                        Assert.AreEqual(3, stack.Frames.Count);
                        Assert.IsInstanceOfType(stack.Frames[0].Target, typeof(PromptDialog.PromptString));
                        Assert.IsInstanceOfType(stack.Frames[1].Target, dialog.Object.GetType());

                        await botData.FlushAsync(default(CancellationToken));
                    }
                });

                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        var botData = scope.Resolve<IBotData>();
                        await botData.LoadAsync(default(CancellationToken));

                        // validate that the fiber state was persisted in IPostToBot.PostAsnc
                        var stack = scope.Resolve<IDialogStack>();
                        Assert.AreEqual(3, stack.Frames.Count);
                    }
                });
            }
        }

        //[TestMethod]
        //public async Task DialogTask_CancellationToken_Propagated_Through_Context()
        //{
        //    var dialog = new Mock<IDialogFrames<object>>(MockBehavior.Strict);

        //    var source = new CancellationTokenSource();

        //    dialog
        //        .Setup(d => d.StartAsync(It.Is<IDialogContext>(c => c.CancellationToken.Equals(source.Token))))
        //        .Returns<IDialogContext>(async context =>
        //        {
        //            context.Wait(dialog.Object.ItemReceived);
        //        });

        //    dialog
        //        .Setup(d => d.ItemReceived(It.Is<IDialogContext>(c => c.CancellationToken.Equals(source.Token)), It.IsAny<IAwaitable<IMessageActivity>>()))
        //        .Returns<IDialogContext, IAwaitable<IMessageActivity>>(async (context, item) =>
        //        {
        //            context.Wait(dialog.Object.ItemReceived);
        //        });

        //    var toBot = MakeTestMessage();

        //    using (new FiberTestBase.ResolveMoqAssembly(dialog.Object))
        //    using (var container = Build(Options.ResolveDialogFromContainer, dialog.Object))
        //    {
        //        var builder = new ContainerBuilder();
        //        builder.RegisterInstance(dialog.Object).As<IDialog<object>>();
        //        builder.Update(container);

        //            using (var scope = DialogModule.BeginLifetimeScope(container, context))
        //            {
        //                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context) =>
        //                {
        //                    await PostActivityAsync(container, toBot, source.Token);
        //                    await PostActivityAsync(container, toBot, source.Token);
        //                }, source.Token);
        //            }
        //        });
        //    }

        //    dialog.VerifyAll();
        //}

        [TestMethod]
        public async Task DialogTask_Frames_After_Poll_No_Post()
        {
            var dialog = new Mock<IDialogFrames<object>>(MockBehavior.Loose);

            dialog
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { PromptDialog.Text(context, dialog.Object.ItemReceived, "blah"); });

            var toBot = MakeTestMessage();

            using (new FiberTestBase.ResolveMoqAssembly(dialog.Object))
            using (var container = Build(Options.None, dialog.Object))
            {
                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        var botData = scope.Resolve<IBotData>();
                        await botData.LoadAsync(default(CancellationToken));

                        var stack = scope.Resolve<IDialogTask>();
                        Assert.AreEqual(0, stack.Frames.Count);

                        // this is modeling a proactive scenario, where we may want to modify the
                        // dialog stack and run some code, but there is no corresponding message
                        // to post to the bot
                        stack.Call(dialog.Object, null);
                        await stack.PollAsync(CancellationToken.None);

                        Assert.AreEqual(2, stack.Frames.Count);
                        Assert.IsInstanceOfType(stack.Frames[0].Target, typeof(PromptDialog.PromptString));
                        Assert.IsInstanceOfType(stack.Frames[1].Target, dialog.Object.GetType());

                        await botData.FlushAsync(default(CancellationToken));
                    }
                });

                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        var botData = scope.Resolve<IBotData>();
                        await botData.LoadAsync(default(CancellationToken));

                        // and now we need to validate that the fiber state was persisted even though
                        // we only called IDialogStack.PollAsync and not IPostToBot.PostAsnc
                        var stack = scope.Resolve<IDialogStack>();
                        Assert.AreEqual(2, stack.Frames.Count);
                    }
                });
            }
        }

        [TestMethod]
        public async Task DialogTask_Forward()
        {
            var dialogOne = new Mock<IDialogFrames<string>>(MockBehavior.Loose);
            var dialogTwo = new Mock<IDialogFrames<string>>(MockBehavior.Loose);
            const string testMessage = "foo";

            dialogOne
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { context.Wait(dialogOne.Object.ItemReceived); });

            dialogOne
                .Setup(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()))
                .Returns<IDialogContext, IAwaitable<IMessageActivity>>(async (context, message) =>
                {
                    var msg = await message;
                    await context.Forward(dialogTwo.Object, dialogOne.Object.ItemReceived<string>, msg, CancellationToken.None);
                });

            dialogOne
                .Setup(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<string>>()))
                .Returns<IDialogContext, IAwaitable<string>>(async (context, message) =>
                {
                    var msg = await message;
                    Assert.AreEqual(testMessage, msg);
                    context.Wait(dialogOne.Object.ItemReceived);
                });


            dialogTwo
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { context.Wait(dialogTwo.Object.ItemReceived); });

            dialogTwo
                .Setup(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()))
                .Returns<IDialogContext, IAwaitable<IMessageActivity>>(async (context, message) =>
                {
                    var msg = await message;
                    context.Done(msg.Text);
                });

            Func<IDialog<object>> MakeRoot = () => dialogOne.Object;
            var toBot = MakeTestMessage();

            using (new FiberTestBase.ResolveMoqAssembly(dialogOne.Object, dialogTwo.Object))
            using (var container = Build(Options.None, dialogOne.Object, dialogTwo.Object))
            {
                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);
                        var task = scope.Resolve<IPostToBot>();
                        toBot.Text = testMessage;
                        await task.PostAsync(toBot, CancellationToken.None);

                        dialogOne.Verify(d => d.StartAsync(It.IsAny<IDialogContext>()), Times.Once);
                        dialogOne.Verify(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()), Times.Once);
                        dialogOne.Verify(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<string>>()), Times.Once);

                        dialogTwo.Verify(d => d.StartAsync(It.IsAny<IDialogContext>()), Times.Once);
                        dialogTwo.Verify(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()), Times.Once);
                    }
                });
            }
        }

        [TestMethod]
        public async Task DialogTask_Frame_Scoring()
        {
            var dialogOne = new Mock<IDialogFrames<string>>(MockBehavior.Loose);
            var dialogTwo = new Mock<IDialogFrames<Guid>>(MockBehavior.Loose);
            var dialogNew = new Mock<IDialogFrames<DateTime>>(MockBehavior.Loose);

            const string TriggerTextTwo = "foo";
            const string TriggerTextNew = "bar";

            // IDialogFrames<T> StartAsync and ItemReceived

            dialogOne
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { context.Call(dialogTwo.Object, dialogOne.Object.ItemReceived); });

            dialogOne
                .Setup(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<Guid>>()))
                .Returns<IDialogContext, IAwaitable<Guid>>(async (context, message) => { context.Wait(dialogOne.Object.ItemReceived); });

            dialogTwo
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { context.Wait(dialogTwo.Object.ItemReceived); });

            dialogTwo
                .Setup(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()))
                .Returns<IDialogContext, IAwaitable<IMessageActivity>>(async (context, message) =>
                {
                    if ((await message).Text == TriggerTextTwo)
                    {
                        context.Done(Guid.NewGuid());
                    }
                    else
                    {
                        context.Wait(dialogTwo.Object.ItemReceived);
                    }
                });

            dialogNew
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { context.Wait(dialogNew.Object.ItemReceived); });

            dialogNew
                .Setup(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()))
                .Returns<IDialogContext, IAwaitable<IMessageActivity>>(async (context, message) => { context.Done(DateTime.UtcNow); });

            // ScoringDialogTask.IScorable

            dialogOne
                .As<IScorable<IActivity, double>>()
                .Setup(s => s.PrepareAsync(It.IsAny<IMessageActivity>(), It.IsAny<CancellationToken>()))
                .Returns<IMessageActivity, CancellationToken>(async (m, t) => m);

            const double scoreOne = 1.0;
            dialogOne
                .As<IScorable<IActivity, double>>()
                .Setup(s => s.HasScore(It.IsAny<IMessageActivity>(), It.IsAny<IMessageActivity>()))
                .Returns<IMessageActivity, IMessageActivity>((m, s) => m.Text == TriggerTextNew);
            dialogOne
                .As<IScorable<IActivity, double>>()
                .Setup(s => s.GetScore(It.IsAny<IMessageActivity>(), It.IsAny<IMessageActivity>()))
                .Returns<IMessageActivity, IMessageActivity>((m, s) => scoreOne);

            dialogTwo
                .As<IScorable<IActivity, double>>()
                .Setup(s => s.PrepareAsync(It.IsAny<IMessageActivity>(), It.IsAny<CancellationToken>()))
                .Returns<IMessageActivity, CancellationToken>(async (m, t) => m);

            const double scoreTwo = 0.5;
            dialogTwo
                .As<IScorable<IActivity, double>>()
                .Setup(s => s.HasScore(It.IsAny<IMessageActivity>(), It.IsAny<IMessageActivity>()))
                .Returns<IMessageActivity, IMessageActivity>((m, s) => m.Text == TriggerTextNew);
            dialogTwo
                .As<IScorable<IActivity, double>>()
                .Setup(s => s.GetScore(It.IsAny<IMessageActivity>(), It.IsAny<IMessageActivity>()))
                .Returns<IMessageActivity, IMessageActivity>((m, s) => scoreTwo);

            Func<IDialog<object>> MakeRoot = () => dialogOne.Object;
            var toBot = MakeTestMessage();

            using (new FiberTestBase.ResolveMoqAssembly(dialogOne.Object, dialogTwo.Object, dialogNew.Object))
            using (var container = Build(Options.None, dialogOne.Object, dialogTwo.Object, dialogNew.Object))
            {
                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);

                        var task = scope.Resolve<IPostToBot>();
                        await scope.Resolve<IBotData>().LoadAsync(default(CancellationToken));
                        var stack = scope.Resolve<IDialogTask>();

                        // set up dialogOne to call dialogNew when triggered
                        dialogOne
                            .As<IScorable<IActivity, double>>()
                            .Setup(s => s.PostAsync(It.IsAny<IMessageActivity>(), It.IsAny<IMessageActivity>(), It.IsAny<CancellationToken>()))
                            .Returns<IMessageActivity, IMessageActivity, CancellationToken>(async (message, state, token) =>
                            {
                                stack.Call(dialogNew.Object.Void(stack), null);
                                await stack.PollAsync(token);
                            });

                        // the stack is empty when we first start
                        Assert.AreEqual(0, stack.Frames.Count);

                        await task.PostAsync(toBot, CancellationToken.None);

                        // now the stack has the looping root frame plus the 1st and 2nd active dialogs
                        // nothing special in the message, so we still have the 1st and 2nd active dialogs
                        Assert.AreEqual(3, stack.Frames.Count);
                        Assert.AreEqual(dialogTwo.Object, stack.Frames[0].Target);
                        Assert.AreEqual(dialogOne.Object, stack.Frames[1].Target);

                        toBot.Text = TriggerTextNew;

                        await task.PostAsync(toBot, CancellationToken.None);

                        // now the trigger has occurred - the interrupting dialog is at the top of the stack,
                        // then the void dialog, then the existing 1st and 2nd dialogs that were interrupted
                        Assert.AreEqual(5, stack.Frames.Count);
                        Assert.AreEqual(dialogNew.Object, stack.Frames[0].Target);
                        Assert.AreEqual(dialogTwo.Object, stack.Frames[2].Target);
                        Assert.AreEqual(dialogOne.Object, stack.Frames[3].Target);

                        toBot.Text = string.Empty;

                        await task.PostAsync(toBot, CancellationToken.None);

                        // now the interrupted dialog will exit, and the void dialog is waiting for original message that
                        // the 2nd dialog had wanted
                        Assert.AreEqual(4, stack.Frames.Count);
                        Assert.AreEqual(dialogTwo.Object, stack.Frames[1].Target);
                        Assert.AreEqual(dialogOne.Object, stack.Frames[2].Target);

                        toBot.Text = TriggerTextTwo;

                        await task.PostAsync(toBot, CancellationToken.None);

                        // and now that the void dialog was able to capture the message, it returns it to the 2nd dialog,
                        // which returns a guid to the 1st dialog
                        Assert.AreEqual(2, stack.Frames.Count);
                        Assert.AreEqual(dialogOne.Object, stack.Frames[0].Target);
                    }
                });
            }

            dialogOne.VerifyAll();
            dialogTwo.VerifyAll();
            dialogNew.VerifyAll();
        }

        public static Mock<IScorable<object, T>> MockScorable<T>(object item, object state, T score, CancellationToken token)
        {
            var scorable = new Mock<IScorable<object, T>>(MockBehavior.Strict);

            scorable
                .Setup(s => s.PrepareAsync(item, token))
                .ReturnsAsync(state);

            scorable
                .Setup(s => s.HasScore(item, state))
                .Returns(true);

            scorable
                .Setup(s => s.GetScore(item, state))
                .Returns(score);

            scorable
                .Setup(s => s.DoneAsync(item, state, token))
                .Returns(Task.CompletedTask);

            return scorable;
        }

        public static async Task DialogTask_Frame_Scoring_Allows_Value(double score)
        {
            var state = new object();
            var item = new Activity();
            var token = new CancellationTokenSource().Token;
            var scorable = MockScorable(item, state, score, token);

            var innerLoop = new Mock<IEventLoop>();
            var innerProducer = new Mock<IEventProducer<IActivity>>();
            var queue = new EventQueue<IActivity>();
            IEventLoop loop = new ScoringEventLoop<double>(innerLoop.Object, innerProducer.Object, queue, new TraitsScorable<IActivity, double>(NormalizedTraits.Instance, Comparer<double>.Default, new[] { scorable.Object }));

            scorable
                .Setup(s => s.PostAsync(item, state, token))
                .Returns(Task.FromResult(0));

            IEventProducer<IActivity> producer = queue;
            producer.Post(item);
            await loop.PollAsync(token);

            scorable.Verify();
        }

        [TestMethod]
        public async Task DialogTask_Frame_Scoring_Allows_Minimum()
        {
            await DialogTask_Frame_Scoring_Allows_Value(0.0);
        }

        [TestMethod]
        public async Task DialogTask_Frame_Scoring_Allows_Maximum()
        {
            await DialogTask_Frame_Scoring_Allows_Value(1.0);
        }

        public static async Task DialogTask_Frame_Scoring_Throws_Out_Of_Range(double score)
        {
            var state = new object();
            var item = new Activity();
            var token = new CancellationTokenSource().Token;
            var scorable = MockScorable(item, state, score, token);

            var innerLoop = new Mock<IEventLoop>();
            var innerProducer = new Mock<IEventProducer<IActivity>>();
            var queue = new EventQueue<IActivity>();
            IEventLoop loop = new ScoringEventLoop<double>(innerLoop.Object, innerProducer.Object, queue, new TraitsScorable<IActivity, double>(NormalizedTraits.Instance, Comparer<double>.Default, new[] { scorable.Object }));

            try
            {
                IEventProducer<IActivity> producer = queue;
                producer.Post(item);
                await loop.PollAsync(token);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            scorable.Verify();
        }

        [TestMethod]
        public async Task DialogTask_Frame_Scoring_Throws_Too_Large()
        {
            await DialogTask_Frame_Scoring_Throws_Out_Of_Range(1.1);
        }

        [TestMethod]
        public async Task DialogTask_Frame_Scoring_Throws_Too_Small()
        {
            await DialogTask_Frame_Scoring_Throws_Out_Of_Range(-0.1);
        }

        [TestMethod]
        public async Task DialogTask_Frame_Scoring_Stops_At_Maximum()
        {
            var state1 = new object();
            var item = new Activity();
            var token = new CancellationTokenSource().Token;
            var scorable1 = MockScorable(item, state1, 1.0, token);
            var scorable2 = new Mock<IScorable<object, double>>(MockBehavior.Strict);

            var innerLoop = new Mock<IEventLoop>();
            var innerProducer = new Mock<IEventProducer<IActivity>>();
            var queue = new EventQueue<IActivity>();
            IEventLoop loop = new ScoringEventLoop<double>(innerLoop.Object, innerProducer.Object, queue, new TraitsScorable<IActivity, double>(NormalizedTraits.Instance, Comparer<double>.Default, new[] { scorable1.Object, scorable2.Object }));

            scorable1
                .Setup(s => s.PostAsync(item, state1, token))
                .Returns(Task.FromResult(0));

            IEventProducer<IActivity> producer = queue;
            producer.Post(item);
            await loop.PollAsync(token);

            scorable1.Verify();
            scorable2.Verify();
        }

        [TestMethod]
        public async Task DialogTask_RememberLastWait()
        {
            var dialogOne = new Mock<IDialogFrames<string>>(MockBehavior.Strict);
            const string testMessage = "foo";

            dialogOne
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { context.Wait(dialogOne.Object.ItemReceived); });

            dialogOne
                .Setup(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()))
                .Returns<IDialogContext, IAwaitable<IMessageActivity>>(async (context, message) =>
                {
                    var msg = await message;
                    var reply = context.MakeMessage();
                    reply.Text = msg.Text;
                    await context.PostAsync(reply);
                    // no need to call context.Wait(...) since frame remembers the last wait from StartAsync(...)
                });

            Func<IDialog<object>> MakeRoot = () => dialogOne.Object;
            var toBot = MakeTestMessage();

            using (new FiberTestBase.ResolveMoqAssembly(dialogOne.Object))
            using (var container = Build(Options.None, dialogOne.Object))
            {
                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);
                        var task = scope.Resolve<IPostToBot>();
                        toBot.Text = testMessage;
                        await task.PostAsync(toBot, CancellationToken.None);

                        dialogOne.Verify(d => d.StartAsync(It.IsAny<IDialogContext>()), Times.Once);
                        dialogOne.Verify(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()), Times.Once);
                    }
                });

                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);
                        var task = scope.Resolve<IPostToBot>();
                        toBot.Text = testMessage;
                        await task.PostAsync(toBot, CancellationToken.None);

                        dialogOne.Verify(d => d.StartAsync(It.IsAny<IDialogContext>()), Times.Once);
                        dialogOne.Verify(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()), Times.Exactly(2));
                    }
                });
            }

        }

        [Serializable]
        public class DialogOne : IDialog
        {
            public async Task StartAsync(IDialogContext context)
            {
                context.Wait(ItemReceived);
            }

            public async Task ItemReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
            {
                await context.Forward(new DialogTwo(), DialogTwoDone, await item, CancellationToken.None);
            }

            public async Task DialogTwoDone(IDialogContext context, IAwaitable<string> item)
            {
                var reply = context.MakeMessage();
                reply.Text = await item;
                await context.PostAsync(reply);
                // no need to wait here because of the frame memory
            }
        }

        [Serializable]
        public class DialogTwo : IDialog<string>
        {
            public async Task StartAsync(IDialogContext context)
            {
                context.Wait(ItemReceived);
            }

            public async Task ItemReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
            {
                var msg = await item;
                context.Done(msg.Text);
            }
        }


        [TestMethod]
        public async Task DialogTask_RememberLastWait_ReturningFromChild()
        {
            string testMessage = "foo";
            Func<IDialog<object>> MakeRoot = () => new DialogOne();
            var toBot = MakeTestMessage();
            toBot.Text = testMessage;


            using (var container = Build(Options.MockConnectorFactory))
            {
                int count = 2;
                var adapter = new TestAdapter();
                await adapter.ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    for (int i = 0; i < count; i++)
                    {
                        using (var scope = DialogModule.BeginLifetimeScope(container, context))
                        {
                            DialogModule_MakeRoot.Register(scope, MakeRoot);
                            var task = scope.Resolve<IPostToBot>();
                            await task.PostAsync(toBot, CancellationToken.None);
                        }
                    }
                });

                var queue = adapter.ActiveQueue;
                Assert.AreEqual(count, queue.Count);
                Assert.AreEqual(testMessage, queue.Dequeue().Text);
            }
        }
    }
}
