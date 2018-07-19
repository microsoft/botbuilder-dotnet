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

using Autofac;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Scorables.Internals;
using Microsoft.Bot.Builder.Classic.Scorables;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using Microsoft.Bot.Builder.Adapters;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    // temporary home for special-purpose IScorable
    public sealed class CancelScorable : ScorableBase<IActivity, double?, double>
    {
        private readonly IDialogStack stack;
        private readonly Regex regex;
        public CancelScorable(IDialogStack stack, Regex regex)
        {
            SetField.NotNull(out this.stack, nameof(stack), stack);
            SetField.NotNull(out this.regex, nameof(regex), regex);
        }

        protected override async Task<double?> PrepareAsync(IActivity item, CancellationToken token)
        {
            var message = item as IMessageActivity;
            if (message != null && message.Text != null)
            {
                var text = message.Text;
                var match = regex.Match(text);
                if (match.Success)
                {
                    return match.Length / ((double)text.Length);
                }
            }

            return null;
        }
        protected override bool HasScore(IActivity item, double? state)
        {
            return state.HasValue;
        }
        protected override double GetScore(IActivity item, double? state)
        {
            return state.Value;
        }
        protected override async Task PostAsync(IActivity item, double? state, CancellationToken token)
        {
            this.stack.Fail(new OperationCanceledException());
        }
        protected override Task DoneAsync(IActivity item, double? state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }

    [TestClass]
    public sealed class CancelScorableTests : PromptTests_Base
    {
        public const string PromptText = "what is your name?";

        [TestMethod]
        public async Task Scorable_Cancel_Not_Triggered()
        {
            var dialog = MockDialog<string>((context, resume) => PromptDialog.Text(context, resume, PromptText));

            using (new FiberTestBase.ResolveMoqAssembly(dialog.Object))
            using (var container = Build(Options.None, dialog.Object))
            using (var containerScope = container.BeginLifetimeScope(
                builder => builder.Register(c => new CancelScorable(c.Resolve<IDialogStack>(), new Regex("cancel"))).As<IScorable<IActivity, double>>()))
            {
                var toBot = MakeTestMessage();
                var context = new TurnContext(new TestAdapter(), (Activity)toBot);

                using (var scope = DialogModule.BeginLifetimeScope(containerScope, context))
                {
                    DialogModule_MakeRoot.Register(scope, () => dialog.Object);

                    var task = scope.Resolve<IPostToBot>();
                    await task.PostAsync(toBot, CancellationToken.None);

                    AssertMentions(PromptText, ((TestAdapter)context.Adapter).GetNextReply().AsMessageActivity());
                }

                using (var scope = DialogModule.BeginLifetimeScope(containerScope, context))
                {
                    DialogModule_MakeRoot.Register(scope, () => dialog.Object);

                    const string TextNormal = "normal response";

                    var task = scope.Resolve<IPostToBot>();
                    toBot.Text = TextNormal;
                    await task.PostAsync(toBot, CancellationToken.None);

                    dialog
                        .Verify(d => d.PromptResult(It.IsAny<IDialogContext>(), It.Is<IAwaitable<string>>(actual => actual.ToTask().Result == TextNormal)));
                }
            }
        }

        [TestMethod]
        public async Task Scorable_Cancel_Is_Triggered()
        {
            var dialog = MockDialog<string>((context, resume) => PromptDialog.Text(context, resume, PromptText));

            using (new FiberTestBase.ResolveMoqAssembly(dialog.Object))
            using (var container = Build(Options.None, dialog.Object))
            using (var containerScope = container.BeginLifetimeScope(
                builder => builder.Register(c => new CancelScorable(c.Resolve<IDialogStack>(), new Regex("cancel"))).As<IScorable<IActivity, double>>()))
            {
                var toBot = MakeTestMessage();
                var context = new TurnContext(new TestAdapter(), (Activity)toBot);

                using (var scope = DialogModule.BeginLifetimeScope(containerScope, context))
                {
                    DialogModule_MakeRoot.Register(scope, () => dialog.Object);

                    var task = scope.Resolve<IPostToBot>();
                    await task.PostAsync(toBot, CancellationToken.None);

                    AssertMentions(PromptText, ((TestAdapter)context.Adapter).GetNextReply().AsMessageActivity());
                }

                using (var scope = DialogModule.BeginLifetimeScope(containerScope, context))
                {
                    DialogModule_MakeRoot.Register(scope, () => dialog.Object);

                    const string TextNormal = "cancel me";

                    var task = scope.Resolve<IPostToBot>();
                    toBot.Text = TextNormal;
                    await task.PostAsync(toBot, CancellationToken.None);

                    dialog
                        .Verify(d => d.PromptResult(It.IsAny<IDialogContext>(), It.Is<IAwaitable<string>>(actual => actual.ToTask().IsFaulted)));
                }
            }
        }
    }

    [Serializable]
    public sealed class CalculatorDialog : IDialog<double>
    {
        async Task IDialog<double>.StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceived);
        }


        // http://stackoverflow.com/a/2196685
        public static double Evaluate(string expression)
        {
            var regex = new Regex(@"([\+\-\*])");

            var text = regex.Replace(expression, " ${1} ")
                            .Replace("/", " div ")
                            .Replace("%", " mod ");

            var xpath = $"number({text})";
            using (var reader = new StringReader("<r/>"))
            {
                var document = new XPathDocument(reader);
                var navigator = document.CreateNavigator();
                var result = navigator.Evaluate(xpath);
                return (double)result;
            }
        }

        public async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            var toBot = await message;
            var value = Evaluate(toBot.Text);
            await context.PostAsync(value.ToString());
            context.Done(value);
        }
    }

    // temporary home for special-purpose IScorable
    public sealed class CalculatorScorable : ScorableBase<IActivity, string, double>
    {
        private readonly IDialogStack stack;
        private readonly Regex regex;
        public CalculatorScorable(IDialogStack stack, Regex regex)
        {
            SetField.NotNull(out this.stack, nameof(stack), stack);
            SetField.NotNull(out this.regex, nameof(regex), regex);
        }

        protected override async Task<string> PrepareAsync(IActivity item, CancellationToken token)
        {
            var message = item as IMessageActivity;
            if (message != null && message.Text != null)
            {
                var text = message.Text;
                var match = regex.Match(text);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            return null;
        }
        protected override bool HasScore(IActivity item, string state)
        {
            return state != null;
        }
        protected override double GetScore(IActivity item, string state)
        {
            return 1.0;
        }
        protected override async Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            var dialog = new CalculatorDialog();

            // let's strip off the prefix in favor of the actual arithmetic expression
            var message = (IMessageActivity)item;
            message.Text = state;

            await this.stack.Forward(dialog.Void(this.stack), null, message, token);
        }
        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }

    [TestClass]
    public sealed class SelectScoreScorableTests : DialogTestBase
    {
        [TestMethod]
        public async Task SelectScoreScorable_Scaling()
        {
            var echo = Chain.PostToChain().Select(msg => $"echo: {msg.Text}").PostToUser().Loop();

            var scorable = new[]
            {
                Actions
                .Bind(async (IBotToUser botToUser, CancellationToken token) =>
                {
                    await botToUser.PostAsync("10");
                })
                .When(new Regex("10.*"))
                .Normalize()
                .SelectScore((r, s) => s * 0.9),

                Actions
                .Bind(async (IBotToUser botToUser, CancellationToken token) =>
                {
                    await botToUser.PostAsync("1");
                })
                .When(new Regex("10.*"))
                .Normalize()
                .SelectScore((r, s) => s * 0.1)

            }.Fold();

            echo = echo.WithScorable(scorable);

            using (var container = Build(Options.ResolveDialogFromContainer, scorable))
            using (var scope = container.BeginLifetimeScope(
                builder => builder.RegisterInstance(echo).As<IDialog<object>>()))
            {
                await AssertScriptAsync(scope,
                    "hello",
                    "echo: hello",
                    "10",
                    "10"
                    );
            }
        }
    }

    [TestClass]
    public sealed class ScorableTriggerTests : DialogTestBase
    {
        public static IScorable<IResolver, double> TriggerAction(Regex regex, Func<IDialog<object>> makeRoot)
        {
            var scorable =
                Actions
                .Bind(async (IDialogStack stack, IMessageActivity activity, CancellationToken token) =>
                {
                    var triggered = makeRoot();
                    stack.Reset();
                    await stack.Forward(triggered.Loop(), null, activity, token);
                })
                .When(regex)
                .Normalize();

            return scorable;
        }

        [TestMethod]
        public async Task TriggerAction()
        {
            var echo = Chain.PostToChain().Select(msg => $"echo: {msg.Text}").PostToUser().Loop();

            Func<IDialog<object>> MakeRootA = () => Chain.PostToChain().Select(msg => $"dialogA: {msg.Text}").PostToUser();
            Func<IDialog<object>> MakeRootB = () => Chain.PostToChain().Select(msg => $"dialogB: {msg.Text}").PostToUser();
            Func<IDialog<object>> MakeRootC = () => Chain.PostToChain().Select(msg => $"dialogC: {msg.Text}").PostToUser();

            var scorable = new[]
            {
                TriggerAction(new Regex(@".*triggerA.*"), MakeRootA),
                TriggerAction(new Regex(@".*triggerB.*"), MakeRootB),
                TriggerAction(new Regex(@".*triggerC.*"), MakeRootC),
            }.Fold();

            using (var container = Build(Options.ResolveDialogFromContainer))
            using (var scope = container.BeginLifetimeScope(
                builder =>
                {
                    builder.RegisterInstance(echo).As<IDialog<object>>();
                    builder.RegisterInstance(scorable).As<IScorable<IResolver, double>>();
                }))
            {
                await AssertScriptAsync(scope,
                    "hello",
                    "echo: hello",
                    "triggerA",
                    "dialogA: triggerA",
                    "stillA",
                    "dialogA: stillA",
                    "triggerB",
                    "dialogB: triggerB",
                    "stillB",
                    "dialogB: stillB",
                    "triggerC",
                    "dialogC: triggerC",
                    "stillC",
                    "dialogC: stillC"
                    );
            }
        }
    }

    [TestClass]
    public sealed class CalculatorScorableTests : DialogTestBase
    {
        [TestMethod]
        public async Task Calculate_Script_Scorable_As_Action_Reset_Stack()
        {
            var echo = Chain.PostToChain().Select(msg => $"echo: {msg.Text}").PostToUser().Loop();

            var scorable = Actions
                .Bind(async (string expression, IDialogStack stack, IMessageActivity activity, CancellationToken token) =>
                {
                    var dialog = new CalculatorDialog();
                    activity.Text = expression;
                    stack.Reset();
                    await stack.Forward(dialog.Loop(), null, activity, token);
                })
                .When(new Regex(@".*calculate\s*(?<expression>.*)"))
                .Normalize();

            echo = echo.WithScorable(scorable);

            using (var container = Build(Options.ResolveDialogFromContainer))
            using (var scope = container.BeginLifetimeScope(
                builder => builder.RegisterInstance(echo).As<IDialog<object>>()))
            {
                await AssertScriptAsync(scope,
                    "hello",
                    "echo: hello",
                    "calculate 2 + 3",
                    "5",
                    "2 + 2",
                    "4"
                    );
            }
        }

        [TestMethod]
        public async Task Calculate_Script_Scorable_As_Action_Interrupt_Stack()
        {
            var echo = Chain.PostToChain().Select(msg => $"echo: {msg.Text}").PostToUser().Loop();

            var scorable = Actions
                .Bind((string expression, IBotData data, IDialogTask stack, IMessageActivity activity, CancellationToken token) =>
                {
                    var dialog = new CalculatorDialog();
                    activity.Text = expression;
                    return stack.InterruptAsync(dialog, activity, token);
                })
                .When(new Regex(@".*calculate\s*(?<expression>.*)"))
                .Normalize();

            echo = echo.WithScorable(scorable);

            using (var container = Build(Options.ResolveDialogFromContainer))
            using (var scope = container.BeginLifetimeScope(
                builder => builder.RegisterInstance(echo).As<IDialog<object>>()))
            {
                await AssertScriptAsync(scope,
                    "hello",
                    "echo: hello",
                    "calculate 2 + 3",
                    "5",
                    "world",
                    "echo: world",
                    "2 + 3",
                    "echo: 2 + 3",
                    "calculate 4 / 2",
                    "2"
                    );
            }
        }

        [TestMethod]
        public async Task Calculate_Script_Scorable_As_Global()
        {
            var echo = Chain.PostToChain().Select(msg => $"echo: {msg.Text}").PostToUser().Loop();

            using (var container = Build(Options.ResolveDialogFromContainer))
            using (var scope = container.BeginLifetimeScope(
                builder =>
                {
                    builder.RegisterInstance(echo).As<IDialog<object>>();
                    builder.Register(c => new CalculatorScorable(c.Resolve<IDialogStack>(), new Regex(@".*calculate\s*(.*)"))).As<IScorable<IActivity, double>>();
                }))
            {
                await AssertScriptAsync(scope,
                    "hello",
                    "echo: hello",
                    "calculate 2 + 3",
                    "5",
                    "world",
                    "echo: world",
                    "2 + 3",
                    "echo: 2 + 3",
                    "calculate 4 / 2",
                    "2"
                    );
            }
        }
    }
}
