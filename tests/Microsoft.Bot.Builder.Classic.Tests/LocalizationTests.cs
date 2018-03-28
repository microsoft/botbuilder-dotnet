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
using System.Linq;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;

using Moq;
using Autofac;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder.Adapters;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public sealed class LocalizationTests : DialogTestBase
    {
        public interface ILocalizedDialog : IDialog<CultureInfo>
        {
            Task FirstMessage(IDialogContext context, IAwaitable<IMessageActivity> message);
        }

        public static CultureInfo CurrentCulture
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture;
            }
        }

        public async static Task UI_Culture_From_Message(string language, CultureInfo current, CultureInfo expected)
        {
            var dialog = new Mock<ILocalizedDialog>();

            CultureInfo actual = null;
            dialog
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async c => { c.Wait(dialog.Object.FirstMessage); });
            dialog
                .Setup(d => d.FirstMessage(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<IMessageActivity>>()))
                .Returns<IDialogContext, IAwaitable<IMessageActivity>>(async (c, m) =>
                {
                    actual = CurrentCulture;
                    c.Wait(dialog.Object.FirstMessage);
                });

            Func<IDialog<object>> MakeRoot = () => dialog.Object;

            using (new FiberTestBase.ResolveMoqAssembly(dialog.Object))
            using (var container = Build(Options.None, dialog.Object))
            {
                var toBot = MakeTestMessage();
                toBot.Locale = language;
                var context = new TurnContext(new TestAdapter(), (Activity)toBot);

                using (var scope = DialogModule.BeginLifetimeScope(container, context))
                {
                    DialogModule_MakeRoot.Register(scope, MakeRoot);

                    var task = scope.Resolve<IPostToBot>();

                    Assert.AreEqual(current, CurrentCulture);
                    await task.PostAsync(toBot, CancellationToken.None);
                    Assert.AreEqual(current, CurrentCulture);
                    Assert.AreEqual(actual, expected);
                }
            }
        }

        [TestMethod]
        public async Task UI_Culture_From_Message_Valid()
        {
            var current = CurrentCulture;
            var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            var expected = cultures.First(c => c != current);

            await UI_Culture_From_Message(expected.Name, current, expected);
        }

        [TestMethod]
        public async Task UI_Culture_From_Message_Invalid()
        {
            string language = "no such language";
            var current = CurrentCulture;
            var expected = current;

            await UI_Culture_From_Message(language, current, expected);
        }
    }
}
