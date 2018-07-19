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
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public sealed class ExceptionTests : DialogTestBase
    {
        [Serializable]
        public sealed class NoResumeHandlerDialog : IDialog<object>
        {
            async Task IDialog<object>.StartAsync(IDialogContext context)
            {
                await Task.Yield();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NoResumeHandlerException))]
        public async Task Exception_DialogStack_NoResumeHandler()
        {
            using (var container = Build(Options.ResolveDialogFromContainer))
            using (var scope = container.BeginLifetimeScope(
                builder => builder.RegisterType<NoResumeHandlerDialog>().AsImplementedInterfaces()))
            {
                await AssertScriptAsync(scope, "hello");
            }
        }

        [Serializable]
        public sealed class ResumeHandlerTwiceDialog : IDialog<object>
        {
            async Task IDialog<object>.StartAsync(IDialogContext context)
            {
                context.Wait(MessageReceived);
                context.Wait(MessageReceived);
            }

            async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> message)
            {
                await Task.Yield();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MultipleResumeHandlerException))]
        public async Task Exception_DialogStack_ResumeHandlerTwice()
        {
            using (var container = Build(Options.ResolveDialogFromContainer))
            using (var scope = container.BeginLifetimeScope(
                builder => builder.RegisterType<ResumeHandlerTwiceDialog>().AsImplementedInterfaces()))
            {
                await AssertScriptAsync(scope, "hello");
            }
        }

        public sealed class NonSerializableDialog : IDialog<object>
        {
            async Task IDialog<object>.StartAsync(IDialogContext context)
            {
                context.Wait(MessageReceived);
            }

            async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> message)
            {
                await Task.Yield();
            }
        }

        [TestMethod]
        public async Task Exception_NonSerializableDialog()
        {
            using (var container = Build(Options.ResolveDialogFromContainer))
            using (var scope = container.BeginLifetimeScope(
                builder => builder.RegisterType<NonSerializableDialog>().AsImplementedInterfaces()))
            {
                try
                {
                    await AssertScriptAsync(scope, "hello");
                    Assert.Fail();
                }
                catch (SerializationException)
                {
                }

            }
        }
    }
}