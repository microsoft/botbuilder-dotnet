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
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Classic.Dialogs.PromptDialog;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    public abstract class PromptTests_Base : DialogTestBase
    {
        public interface IPromptCaller<T> : IDialog<object>
        {
            Task FirstMessage(IDialogContext context, IAwaitable<IMessageActivity> message);
            Task PromptResult(IDialogContext context, IAwaitable<T> result);
        }

        public static Mock<IPromptCaller<T>> MockDialog<T>(Action<IDialogContext, ResumeAfter<T>> prompt)
        {
            var dialog = new Moq.Mock<IPromptCaller<T>>(MockBehavior.Strict);
            dialog
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async c => { c.Wait(dialog.Object.FirstMessage); });
            dialog
                .Setup(d => d.FirstMessage(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()))
                .Returns<IDialogContext, IAwaitable<object>>(async (c, a) => { prompt(c, dialog.Object.PromptResult); });
            dialog
                .Setup(d => d.PromptResult(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<T>>()))
                .Returns<IDialogContext, IAwaitable<T>>(async (c, a) => { c.Done(default(T)); });

            return dialog;
        }
    }

    [TestClass]
    public sealed class PromptTests_Localization
    {
        [TestMethod]
        public async Task PromptLocalization_ChangeCulture()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var options = PromptDialog.PromptConfirm.Options;
            var patterns = PromptDialog.PromptConfirm.Patterns;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("es-ES");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("es-ES");

            Assert.AreNotEqual(options, PromptDialog.PromptConfirm.Options);
            Assert.AreNotEqual(patterns, PromptDialog.PromptConfirm.Patterns);
        }
    }

    [TestClass]
    public sealed class PromptTests_Success : PromptTests_Base
    {
        private const string PromptText = "hello there";

        public async Task PromptSuccessAsync<T>(Action<IDialogContext, ResumeAfter<T>> prompt, string text, T expected, string locale = null)
        {
            var toBot = MakeTestMessage();
            toBot.Text = text;
            toBot.Locale = locale;
            await PromptSuccessAsync(prompt, toBot, a => a.Equals(expected));
        }

        public async Task PromptSuccessAsync<T>(Action<IDialogContext, ResumeAfter<T>> prompt, IMessageActivity toBot, Func<T, bool> expected)
        {
            var dialogRoot = MockDialog<T>(prompt);

            Func<IDialog<object>> MakeRoot = () => dialogRoot.Object;

            using (new FiberTestBase.ResolveMoqAssembly(dialogRoot.Object))
            using (var container = Build(Options.ScopedQueue, dialogRoot.Object))
            {
                var adapter = new TestAdapter();
                await adapter.ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);
                        var task = scope.Resolve<IPostToBot>();
                        await task.PostAsync(toBot, CancellationToken.None);
                        AssertMentions(PromptText, adapter.ActiveQueue.Dequeue() as IMessageActivity);
                    }
                });

                await adapter.ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);

                        var task = scope.Resolve<IPostToBot>();
                        await task.PostAsync(toBot, CancellationToken.None);
                        AssertNoMessages(scope);
                        dialogRoot.Verify(d => d.PromptResult(It.IsAny<IDialogContext>(), It.Is<IAwaitable<T>>(actual => expected(actual.GetAwaiter().GetResult()))), Times.Once);
                    }
                });
            }
        }

        [TestMethod]
        public async Task PromptSuccess_Attachment()
        {
            var jpgAttachment = new Attachment { ContentType = "image/jpeg", Content = "http://a.jpg" };
            var bJpgAttachment = new Attachment { ContentType = "image/jpeg", Content = "http://b.jpg" };
            var pdfAttachment = new Attachment { ContentType = "application/pdf", Content = "http://a.pdf" };
            var toBot = MakeTestMessage();
            toBot.Attachments = new List<Attachment>
            {
                jpgAttachment,
                pdfAttachment
            };
            await PromptSuccessAsync<IEnumerable<Attachment>>((context, resume) => PromptDialog.Attachment(context, resume, PromptText), toBot, actual => new[] { jpgAttachment, pdfAttachment }.SequenceEqual(actual));
            await PromptSuccessAsync<IEnumerable<Attachment>>((context, resume) => PromptDialog.Attachment(context, resume, PromptText, new[] { "image/jpeg" }), toBot, actual => new[] { jpgAttachment }.SequenceEqual(actual));
            await PromptSuccessAsync<IEnumerable<Attachment>>((context, resume) => PromptDialog.Attachment(context, resume, PromptText, new[] { "application/pdf" }), toBot, actual => new[] { pdfAttachment }.SequenceEqual(actual));
            await PromptSuccessAsync<IEnumerable<Attachment>>((context, resume) => PromptDialog.Attachment(context, resume, PromptText, new[] { "image/jpeg", "application/pdf" }), toBot, actual => new[] { jpgAttachment, pdfAttachment }.SequenceEqual(actual));
            toBot.Attachments.Add(bJpgAttachment);
            await PromptSuccessAsync<IEnumerable<Attachment>>((context, resume) => PromptDialog.Attachment(context, resume, PromptText, new[] { "image/jpeg" }), toBot, actual => new[] { jpgAttachment, bJpgAttachment }.SequenceEqual(actual));
        }

        [TestMethod]
        public async Task PromptSuccess_Text()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Text(context, resume, PromptText), "lol wut", "lol wut");
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_Yes()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Confirm(context, resume, PromptText, promptStyle: PromptStyle.None), "yes", true);
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_Yes_CaseInsensitive()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Confirm(context, resume, PromptText, promptStyle: PromptStyle.None), "Yes", true);
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_Yes_WithLocale()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Confirm(context, resume, PromptText, promptStyle: PromptStyle.None), "seguro", true, "es-ES");
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_No()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Confirm(context, resume, PromptText, promptStyle: PromptStyle.None), "no", false);
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_No_CaseInsensitive()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Confirm(context, resume, PromptText, promptStyle: PromptStyle.None), "No", false);
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_No_WithLocale()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Confirm(context, resume, PromptText, promptStyle: PromptStyle.None), "nop", false, "es-ES");
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_Maybe()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Confirm(context, resume, PromptText, promptStyle: PromptStyle.None,
                options: new string[] { "maybe", "no" }, patterns: new string[][] { new string[] { "maybe" }, new string[] { "no" } }),
                "maybe", true);
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_Maybe_WithLocale()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Confirm(context, resume, PromptText, promptStyle: PromptStyle.None,
                options: new string[] { "quizás", "nunca" }, patterns: new string[][] { new string[] { "quizás" }, new string[] { "nunca" } }),
                "quizás", true, "es-ES");
        }

        [TestMethod]
        public async Task PromptSuccess_Number_Long()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Number(context, resume, PromptText), "42", 42L);
        }

        [TestMethod]
        public async Task PromptSuccess_Number_Long_Text()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Number(context, resume, PromptText), "ten", 10L);
        }

        [TestMethod]
        public async Task PromptSuccess_Number_Long_TextLocale()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Number(context, resume, PromptText), "diez", 10L, "es-ES");
        }

        [TestMethod]
        public async Task PromptSuccess_Number_Double()
        {
            await PromptSuccessAsync((context, resume) => PromptDialog.Number(context, resume, PromptText), "42", 42d);
        }

        [TestMethod]
        public async Task PromptSuccess_Choice()
        {
            var choices = new[] { "one", "two", "three" };
            await PromptSuccessAsync((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText, promptStyle: PromptStyle.None), "two", "two");
        }

        [TestMethod]
        public async Task PromptSuccess_Choice_MessageCaseInsensitive()
        {
            var choices = new[] { "one", "two", "three" };
            await PromptSuccessAsync((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText, promptStyle: PromptStyle.None), "Two", "two");
        }

        [TestMethod]
        public async Task PromptSuccess_Choice_OptionsCaseInsensitive()
        {
            var choices = new[] { "One", "Two", "Three" };
            await PromptSuccessAsync((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText, promptStyle: PromptStyle.None), "two", "Two");
        }

        [TestMethod]
        public async Task PromptSuccess_Choice_Ordinal()
        {
            var choices = new[] { "one", "two", "three" };
            await PromptSuccessAsync((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText, promptStyle: PromptStyle.None), "second", "two");
        }

        [TestMethod]
        public async Task PromptSuccess_Choice_Ordinal_WithLocale()
        {
            var choices = new[] { "one", "two", "three" };
            await PromptSuccessAsync((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText, promptStyle: PromptStyle.None), "el segundo", "two", "es-ES");
        }

        [TestMethod]
        public async Task PromptSuccess_Choice_Reverse_Ordinal()
        {
            var choices = new[] { "one", "two", "three" };
            await PromptSuccessAsync((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText, promptStyle: PromptStyle.None), "the third from last", "one");
        }

        [TestMethod]
        public async Task PromptSuccess_Choice_Reverse_Ordinal_SpecialCase()
        {
            var choices = new[] { "one", "two", "three" };
            await PromptSuccessAsync((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText, promptStyle: PromptStyle.None), "the last one", "three");
        }

        [TestMethod]
        public async Task PromptSuccess_Choice_Reverse_Ordinal_WithLocale()
        {
            var choices = new[] { "one", "two", "three" };
            await PromptSuccessAsync((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText, promptStyle: PromptStyle.None), "el antepenúltimo", "one", "es-ES");
        }

        [TestMethod]
        public async Task PromptSuccess_Choice_Cardinal()
        {
            var choices = new[] { "one", "two", "three" };
            await PromptSuccessAsync((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText, promptStyle: PromptStyle.None), "2", "two");
        }

        [TestMethod]
        public async Task PromptSuccess_Choice_Overlapping()
        {
            var choices = new[] { "9", "19", "else" };
            await PromptSuccessAsync((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText, promptStyle: PromptStyle.None), "9", "9");
        }

        [TestMethod]
        public async Task PromptSuccess_Choice_Overlapping_Reverse()
        {
            var choices = new[] { "19", "9", "else" };
            await PromptSuccessAsync((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText, promptStyle: PromptStyle.None), "9", "9");
        }

        [TestMethod]
        public async Task PromptSuccess_Choice_PartialMatch()
        {
            var choices = new[] { "hotel resort", "hotel spa", "hotel premium" };
            await PromptSuccessAsync((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText, promptStyle: PromptStyle.None), "resort", "hotel resort");
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        [DeploymentItem(@"Scripts\ChoiceDescriptions.script")]
        public async Task PromptSuccess_Choice_Descriptions()
        {
            var choices = new[] { "19", "9", "else" };
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            await Script.VerifyDialogScript(pathScript,
                new PromptChoice<string>(choices, PromptText, null, 0, promptStyle: PromptStyle.Auto, descriptions: new List<string>() { "choice19", "choice9", "choiceelse" }), true, "9");
        }


        [TestMethod]
        [DeploymentItem(@"Scripts\ChoiceDescriptionsRetry.script")]
        public async Task PromptRetry_Choice_Descriptions()
        {
            var choices = new[] { "19", "9", "else" };
            var pathScript = TestFiles.DeploymentItemPathsForCaller(TestContext, this.GetType()).Single();
            await Script.VerifyDialogScript(pathScript,
                new PromptChoice<string>(choices, PromptText, null, 1, promptStyle: PromptStyle.Auto, descriptions: new List<string>() { "choice19", "choice9", "choiceelse" }), true, "10", "9");
        }
    }

    [TestClass]
    public sealed class PromptTests_Failure : PromptTests_Base
    {
        private const string PromptText = "hello there";
        private const string RetryText = "hello there again";
        private const int MaximumAttempts = 1;

        public async Task PromptFailureAsync<T>(Action<IDialogContext, ResumeAfter<T>> prompt)
        {
            var dialogRoot = MockDialog<T>(prompt);

            Func<IDialog<object>> MakeRoot = () => dialogRoot.Object;
            var toBot = MakeTestMessage();

            using (new FiberTestBase.ResolveMoqAssembly(dialogRoot.Object))
            using (var container = Build(Options.ScopedQueue, dialogRoot.Object))
            {
                var adapter = new TestAdapter();
                await adapter.ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);

                        var task = scope.Resolve<IPostToBot>();

                        await task.PostAsync(toBot, CancellationToken.None);
                        AssertMentions(PromptText, adapter.ActiveQueue.Dequeue() as IMessageActivity);
                    }
                });

                await adapter.ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);

                        var task = scope.Resolve<IPostToBot>();

                        await task.PostAsync(toBot, CancellationToken.None);
                        AssertMentions(RetryText, adapter.ActiveQueue.Dequeue() as IMessageActivity);
                    }
                });

                await adapter.ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);

                        var task = scope.Resolve<IPostToBot>();

                        await task.PostAsync(toBot, CancellationToken.None);
                        AssertMentions("too many attempts", adapter.ActiveQueue.Dequeue() as IMessageActivity);
                        dialogRoot.Verify(d => d.PromptResult(It.IsAny<IDialogContext>(), It.Is<IAwaitable<T>>(actual => actual.ToTask().IsFaulted)), Times.Once);
                    }
                });
            }
        }

        [TestMethod]
        public async Task PromptFailure_Number()
        {
            await PromptFailureAsync<long>((context, resume) => PromptDialog.Number(context, resume, PromptText, RetryText, MaximumAttempts));
        }

        [TestMethod]
        public async Task PromptFailure_Choice()
        {
            var choices = new[] { "one", "two", "three" };
            await PromptFailureAsync<string>((context, resume) => PromptDialog.Choice(context, resume, choices, PromptText, RetryText, MaximumAttempts, promptStyle: PromptStyle.None));
        }

        [TestMethod]
        public async Task PromptFailure_Confirm()
        {
            await PromptFailureAsync<bool>((context, resume) => PromptDialog.Confirm(context, resume, PromptText, RetryText, MaximumAttempts, promptStyle: PromptStyle.None));
        }

        [TestMethod]
        public async Task PromptFailure_Attachment()
        {
            await PromptFailureAsync<IEnumerable<Attachment>>((context, resume) => PromptDialog.Attachment(context, resume, PromptText, retry: RetryText, attempts: MaximumAttempts));
        }
    }
}
