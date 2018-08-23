using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Classic.Base;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public class DialogTaskManagerTests : DialogTestBase
    {
        [TestMethod]
        public async Task DialogTaskManager_DefaultDialogTask()
        {
            var dialog = new Mock<DialogTaskTests.IDialogFrames<string>>(MockBehavior.Strict);

            dialog
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { context.Wait(dialog.Object.ItemReceived); });

            dialog
                .Setup(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()))
                .Returns<IDialogContext, IAwaitable<IMessageActivity>>(async (context, message) =>
                {
                    var msg = await message;
                    await context.PostAsync(msg.Text);
                    context.Wait(dialog.Object.ItemReceived);
                });

            Func<IDialog<object>> MakeRoot = () => dialog.Object;
            var toBot = MakeTestMessage();

            using (new FiberTestBase.ResolveMoqAssembly(dialog.Object))
            using (var container = Build(Options.None, dialog.Object))
            {
                string foo = "foo";
                string bar = "bar";
                toBot.Text = foo;
                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);
                        var botData = scope.Resolve<IBotData>();
                        await botData.LoadAsync(CancellationToken.None);
                        var stack = scope.Resolve<IDialogStack>();
                        await scope.Resolve<IPostToBot>().PostAsync(toBot, CancellationToken.None);
                        var dialogTaskManager = scope.Resolve<IDialogTaskManager>();
                        Assert.AreEqual(1, dialogTaskManager.DialogTasks.Count);
                        Assert.AreEqual(stack, dialogTaskManager.DialogTasks[0]);

                        // check if the task records are persisted!
                        Assert.IsTrue(botData.PrivateConversationData.ContainsKey(DialogModule.BlobKey));
                        Assert.IsFalse(
                            botData.PrivateConversationData.ContainsKey(DialogModule.BlobKey +
                                                                          dialogTaskManager.DialogTasks.Count));
                        var queue = ((TestAdapter)context.Adapter).ActiveQueue;
                        Assert.AreEqual(foo, queue.Dequeue().Text);
                    }
                });


                //create another dialog task and make sure that it is not overriding the default dialog task
                toBot.Text = bar;
                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);
                        var botData = scope.Resolve<IBotData>();
                        await botData.LoadAsync(CancellationToken.None);
                        var dialogTaskManager = scope.Resolve<IDialogTaskManager>();
                        Assert.AreEqual(1, dialogTaskManager.DialogTasks.Count);

                        var task = dialogTaskManager.CreateDialogTask();
                        Assert.AreEqual(2, dialogTaskManager.DialogTasks.Count);
                        await botData.FlushAsync(CancellationToken.None);
                        var post = scope.Resolve<IPostToBot>();
                        await post.PostAsync(toBot, CancellationToken.None);
                        var queue = ((TestAdapter)context.Adapter).ActiveQueue;
                        Assert.AreEqual(bar, queue.Dequeue().Text);
                    }
                });
            }
        }


        [TestMethod]
        public async Task DialogTaskManager_TwoParallelDialogTasks()
        {
            var dialog = new Mock<DialogTaskTests.IDialogFrames<string>>(MockBehavior.Strict);
            var secondStackDialog = new Mock<DialogTaskTests.IDialogFrames<object>>(MockBehavior.Strict);

            dialog
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { context.Wait(dialog.Object.ItemReceived); });

            dialog
                .Setup(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()))
                .Returns<IDialogContext, IAwaitable<IMessageActivity>>(async (context, message) =>
                {
                    var msg = await message;
                    await context.PostAsync(msg.Text);
                    context.Wait(dialog.Object.ItemReceived);
                });


            var promptMessage = "Say something!";
            secondStackDialog
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(
                    async context =>
                        { context.Wait(secondStackDialog.Object.ItemReceived<IMessageActivity>); });

            secondStackDialog
                .Setup(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()))
                .Returns<IDialogContext, IAwaitable<IMessageActivity>>(async (context, message) =>
                {
                    PromptDialog.Text(context, secondStackDialog.Object.ItemReceived, promptMessage);
                });

            secondStackDialog
                .Setup(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<string>>()))
                .Returns<IDialogContext, IAwaitable<string>>(async (context, message) =>
                {
                    await context.PostAsync($"from prompt {await message}");
                    context.Wait(secondStackDialog.Object.ItemReceived<IMessageActivity>);
                });

            Func<IDialog<object>> makeRoot = () => dialog.Object;
            Func<IDialog<object>> secondStackMakeRoot = () => secondStackDialog.Object;
            var toBot = MakeTestMessage();

            using (new FiberTestBase.ResolveMoqAssembly(dialog.Object, secondStackDialog.Object))
            using (var container = Build(Options.MockConnectorFactory, dialog.Object, secondStackDialog.Object))
            {
                string foo = "foo";
                string bar = "bar";
                toBot.Text = foo;
                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, makeRoot);
                        await scope.Resolve<IPostToBot>().PostAsync(toBot, CancellationToken.None);

                        var botData = scope.Resolve<IBotData>();
                        await botData.LoadAsync(CancellationToken.None);
                        var dialogTaskManager = scope.Resolve<IDialogTaskManager>();

                        //create second dialog task
                        var secondDialogTask = dialogTaskManager.CreateDialogTask();
                        var reactiveDialogTask = new ReactiveDialogTask(secondDialogTask, secondStackMakeRoot);
                        IEventLoop loop = reactiveDialogTask;
                        await loop.PollAsync(CancellationToken.None);
                        IEventProducer<IActivity> producer = reactiveDialogTask;
                        producer.Post(toBot);
                        await loop.PollAsync(CancellationToken.None);
                        await botData.FlushAsync(CancellationToken.None);

                        var queue = ((TestAdapter)context.Adapter).ActiveQueue;
                        Assert.AreEqual(foo, queue.Dequeue().Text);
                        Assert.AreEqual(promptMessage, queue.Dequeue().Text);
                    }
                });

                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        toBot.Text = bar;
                        DialogModule_MakeRoot.Register(scope, makeRoot);
                        await scope.Resolve<IPostToBot>().PostAsync(toBot, CancellationToken.None);

                        var dialogTaskManager = scope.Resolve<IDialogTaskManager>();
                        Assert.AreEqual(2, dialogTaskManager.DialogTasks.Count);
                        var secondDialogTask = dialogTaskManager.DialogTasks[1];
                        await secondDialogTask.PollAsync(CancellationToken.None);
                        secondDialogTask.Post(toBot);
                        await secondDialogTask.PollAsync(CancellationToken.None);

                        var queue = ((TestAdapter)context.Adapter).ActiveQueue;
                        Assert.AreEqual(bar, queue.Dequeue().Text);
                        Assert.AreEqual($"from prompt {bar}", queue.Dequeue().Text);
                    }
                });
            }
        }
    }
}
