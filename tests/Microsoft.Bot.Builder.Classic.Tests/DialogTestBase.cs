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
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    public abstract class DialogTestBase
    {
        [Flags]
        public enum Options { None = 0, Reflection = 1, ScopedQueue = 2, MockConnectorFactory = 4, ResolveDialogFromContainer = 8, LastWriteWinsCachingBotDataStore = 16 };

        public static IContainer Build(Options options, params object[] singletons)
        {
            var builder = new ContainerBuilder();
            if (options.HasFlag(Options.ResolveDialogFromContainer))
            {
                builder.RegisterModule(new DialogModule());
            }
            else
            {
                builder.RegisterModule(new DialogModule_MakeRoot());
            }

            // make a "singleton" MockConnectorFactory per unit test execution
            IConnectorClientFactory factory = null;
            builder
                .Register((c, p) => factory ?? (factory = new MockConnectorFactory(c.Resolve<IAddress>().BotId)))
                .As<IConnectorClientFactory>()
                .InstancePerLifetimeScope();

            if (options.HasFlag(Options.Reflection))
            {
                builder.RegisterModule(new ReflectionSurrogateModule());
            }

            foreach (var singleton in singletons)
            {
                builder
                    .Register(c => singleton)
                    .Keyed(FiberModule.Key_DoNotSerialize, singleton.GetType());
            }

            return builder.Build();
        }

        public static class ChannelID
        {
            public const string User = "testUser";
            public const string Bot = "testBot";
        }

        public static IMessageActivity MakeTestMessage()
        {
            return new Activity()
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityTypes.Message,
                From = new ChannelAccount { Id = ChannelID.User },
                Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString() },
                Recipient = new ChannelAccount { Id = ChannelID.Bot },
                ServiceUrl = "InvalidServiceUrl",
                ChannelId = "Test",
                Attachments = Array.Empty<Attachment>(),
                Entities = Array.Empty<Microsoft.Bot.Schema.Entity>(),
            };
        }


        public static async Task AssertScriptAsync(ILifetimeScope container, params string[] pairs)
        {
            Assert.AreNotEqual(0, pairs.Length);

            var toBot = MakeTestMessage();

            for (int index = 0; index < pairs.Length; ++index)
            {
                var toBotText = pairs[index];
                toBot.Text = toBotText;

                await new TestAdapter().ProcessActivityAsync((Activity)toBot, async (context, cancellationToken) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        var task = scope.Resolve<IPostToBot>();
                        await task.PostAsync(toBot, CancellationToken.None);

                        var queue = ((TestAdapter)context.Adapter).ActiveQueue;

                        // if user has more to say, bot should have said something
                        if (index + 1 < pairs.Length)
                        {
                            Assert.AreNotEqual(0, queue.Count);
                        }

                        while (queue.Count > 0)
                        {
                            ++index;

                            var toUser = queue.Dequeue();
                            string actual;
                            switch (toUser.Type)
                            {
                                case ActivityTypes.Message:
                                    actual = toUser.Text;
                                    break;
                                case ActivityTypes.EndOfConversation:
                                    actual = toUser.AsEndOfConversationActivity().Code;
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                            var expected = pairs[index];

                            Assert.AreEqual(expected, actual);
                        }
                    }
                });
            }
        }

        public static void AssertMentions(string expectedText, IMessageActivity actualToUser)
        {
            var index = actualToUser.Text.IndexOf(expectedText, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(index >= 0);
        }

        public static void AssertNoMessages(ILifetimeScope scope)
        {
            var queue = ((TestAdapter)scope.Resolve<Microsoft.Bot.Builder.ITurnContext>().Adapter).ActiveQueue;
            Assert.AreEqual(0, queue.Count);
        }

        public static string NewID()
        {
            return Guid.NewGuid().ToString();
        }

        public static async Task AssertOutgoingActivity(ILifetimeScope container, Action<IMessageActivity> asserts)
        {
            var queue = ((TestAdapter)container.Resolve<Microsoft.Bot.Builder.ITurnContext>().Adapter).ActiveQueue;

            if (queue.Count != 1)
            {
                Assert.Fail("Expecting only 1 activity");
            }

            var toUser = queue.Dequeue();

            asserts(toUser);
        }
    }
}
