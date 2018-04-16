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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public sealed class ChainTests : DialogTestBase
    {
        public static void AssertQueryText(string expectedText, Queue<Activity> queue)
        {
            var texts = queue.Select(m => m.Text).ToArray();
            // last message is re-prompt, next-to-last is result of query expression
            var actualText = texts.Reverse().ElementAt(1);
            Assert.AreEqual(expectedText, actualText);
        }

        public static IDialog<string> MakeSelectManyQuery()
        {
            var prompts = new[] { "p1", "p2", "p3" };

            var query = from x in new PromptDialog.PromptString(prompts[0], prompts[0], attempts: 1)
                        from y in new PromptDialog.PromptString(prompts[1], prompts[1], attempts: 1)
                        from z in new PromptDialog.PromptString(prompts[2], prompts[2], attempts: 1)
                        select string.Join(" ", x, y, z);

            query = query.PostToUser();

            return query;
        }

        [TestMethod]
        public async Task LinqQuerySyntax_SelectMany()
        {
            var toBot = MakeTestMessage();

            var words = new[] { "hello", "world", "!" };

            using (var container = Build(Options.Reflection))
            {
                var adapter = new TestAdapter();

                foreach (var word in words)
                {
                    toBot.Text = word;
                    await adapter.ProcessActivity((Activity)toBot, async (context) =>
                    {
                        using (var scope = DialogModule.BeginLifetimeScope(container, context))
                        {
                            DialogModule_MakeRoot.Register(scope, MakeSelectManyQuery);

                            var task = scope.Resolve<IPostToBot>();
                            // if we inline the query from MakeQuery into this method, and we use an anonymous method to return that query as MakeRoot
                            // then because in C# all anonymous functions in the same method capture all variables in that method, query will be captured
                            // with the linq anonymous methods, and the serializer gets confused trying to deserialize it all.
                            await task.PostAsync(toBot, CancellationToken.None);
                        }
                    });
                }

                var expected = string.Join(" ", words);
                AssertQueryText(expected, adapter.ActiveQueue);
            }
        }

        public static IDialog<string> MakeSelectQuery()
        {
            const string Prompt = "p1";

            var query = from x in new PromptDialog.PromptString(Prompt, Prompt, attempts: 1)
                        let w = new string(x.Reverse().ToArray())
                        select w;

            query = query.PostToUser();

            return query;
        }

        [TestMethod]
        public async Task LinqQuerySyntax_Select()
        {
            const string Phrase = "hello world";

            using (var container = Build(Options.Reflection))
            {
                var toBot = MakeTestMessage();
                toBot.Text = Phrase;
                var adapter = new TestAdapter();
                await adapter.ProcessActivity((Activity)toBot, async (context) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeSelectQuery);

                        var task = scope.Resolve<IPostToBot>();
                        await task.PostAsync(toBot, CancellationToken.None);
                    }
                });

                var expected = new string(Phrase.Reverse().ToArray());
                AssertQueryText(expected, adapter.ActiveQueue);
            }
        }

        [TestMethod]
        public async Task LinqQuerySyntax_Where_True()
        {
            var query = Chain.PostToChain().Select(m => m.Text).Where(text => text == true.ToString()).PostToUser();

            using (var container = Build(Options.Reflection))
            {
                var toBot = MakeTestMessage();
                toBot.Text = true.ToString();

                var adapter = new TestAdapter();
                await adapter.ProcessActivity((Activity)toBot, async (context) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, () => query);

                        var task = scope.Resolve<IPostToBot>();
                        await task.PostAsync(toBot, CancellationToken.None);
                    }
                });

                var texts = adapter.ActiveQueue.Select(m => m.Text).ToArray();
                Assert.AreEqual(1, texts.Length);
                Assert.AreEqual(true.ToString(), texts[0]);
            }
        }

        [TestMethod]
        public async Task LinqQuerySyntax_Where_False()
        {
            var query = Chain.PostToChain().Select(m => m.Text).Where(text => text == true.ToString()).PostToUser();

            using (var container = Build(Options.Reflection))
            {
                var toBot = MakeTestMessage();
                toBot.Text = false.ToString();
                var adapter = new TestAdapter();
                await adapter.ProcessActivity((Activity)toBot, async (context) =>
                {

                    using (var scope = DialogModule.BeginLifetimeScope(container, context))
                    {
                        DialogModule_MakeRoot.Register(scope, () => query);

                        var task = scope.Resolve<IPostToBot>();
                        try
                        {
                            await task.PostAsync(toBot, CancellationToken.None);
                            Assert.Fail();
                        }
                        catch (Chain.WhereCanceledException)
                        {
                        }
                    }
                });

                var texts = adapter.ActiveQueue.Select(m => m.Text).ToArray();
                Assert.AreEqual(1, texts.Length);
                Func<string, string, bool> Contains = (text, q) => text.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
                Assert.IsTrue(Contains(texts[0], "exception") || Contains(texts[0], "bot code is having an issue"));
            }
        }

        public static IDialog<string> MakeSwitchDialog()
        {
            var toBot = from message in Chain.PostToChain() select message.Text;

            var logic =
                toBot
                .Switch
                (
                    new RegexCase<string>(new Regex("^hello"), (context, text) =>
                    {
                        return "world!";
                    }),
                    new Case<string, string>((txt) => txt == "world", (context, text) =>
                    {
                        return "!";
                    }),
                    new DefaultCase<string, string>((context, text) =>
                    {
                        return text;
                    }
                )
            );

            var toUser = logic.PostToUser();

            return toUser;
        }

        [TestMethod]
        public async Task Chain_Switch_Case()
        {
            var toBot = MakeTestMessage();

            var words = new[] { "hello", "world", "echo" };
            var expectedReply = new[] { "world!", "!", "echo" };

            using (var container = Build(Options.Reflection))
            {
                var adapter = new TestAdapter();
                foreach (var word in words)
                {
                    toBot.Text = word;
                    await adapter.ProcessActivity((Activity)toBot, async (context) =>
                    {
                        using (var scope = DialogModule.BeginLifetimeScope(container, context))
                        {
                            DialogModule_MakeRoot.Register(scope, MakeSwitchDialog);

                            var task = scope.Resolve<IPostToBot>();
                            await task.PostAsync(toBot, CancellationToken.None);
                        }
                    });
                }

                var texts = adapter.ActiveQueue.Select(m => m.Text).ToArray();
                CollectionAssert.AreEqual(expectedReply, texts);
            }
        }

        public static IDialog<string> MakeUnwrapQuery()
        {
            const string Prompt1 = "p1";
            const string Prompt2 = "p2";
            return new PromptDialog.PromptString(Prompt1, Prompt1, attempts: 1).Select(p => new PromptDialog.PromptString(Prompt2, Prompt2, attempts: 1)).Unwrap().PostToUser();
        }

        [TestMethod]
        public async Task Linq_Unwrap()
        {
            var toBot = MakeTestMessage();

            var words = new[] { "hello", "world" };

            using (var container = Build(Options.Reflection))
            {
                var adapter = new TestAdapter();
                foreach (var word in words)
                {
                    toBot.Text = word;
                    await adapter.ProcessActivity((Activity)toBot, async (context) =>
                    {

                        using (var scope = DialogModule.BeginLifetimeScope(container, context))
                        {
                            DialogModule_MakeRoot.Register(scope, MakeUnwrapQuery);

                            var task = scope.Resolve<IPostToBot>();
                            await task.PostAsync(toBot, CancellationToken.None);
                        }
                    });
                }

                var expected = words.Last();
                AssertQueryText(expected, adapter.ActiveQueue);
            }
        }

        [TestMethod]
        public async Task LinqQuerySyntax_Without_Reflection_Surrogate()
        {
            // no environment capture in closures here
            var query = from x in new PromptDialog.PromptString("p1", "p1", 1)
                        from y in new PromptDialog.PromptString("p2", "p2", 1)
                        select string.Join(" ", x, y);

            query = query.PostToUser();

            var words = new[] { "hello", "world" };

            using (var container = Build(Options.None))
            {
                var toBot = MakeTestMessage();

                var adapter = new TestAdapter();
                foreach (var word in words)
                {
                    toBot.Text = word;
                    await adapter.ProcessActivity((Activity)toBot, async (context) =>
                    {
                        using (var scope = DialogModule.BeginLifetimeScope(container, context))
                        {
                            DialogModule_MakeRoot.Register(scope, () => query);

                            var task = scope.Resolve<IPostToBot>();
                            await task.PostAsync(toBot, CancellationToken.None);
                        }
                    });
                }

                var expected = string.Join(" ", words);
                AssertQueryText(expected, adapter.ActiveQueue);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ClosureCaptureException))]
        public async Task LinqQuerySyntax_Throws_ClosureCaptureException()
        {
            var prompts = new[] { "p1", "p2" };
            var query = new PromptDialog.PromptString(prompts[0], prompts[0], attempts: 1).Select(p => new PromptDialog.PromptString(prompts[1], prompts[1], attempts: 1)).Unwrap().PostToUser();

            using (var container = Build(Options.None))
            {
                var formatter = container.Resolve<IFormatter>();
                using (var stream = new MemoryStream())
                {
                    formatter.Serialize(stream, query);
                }
            }
        }

        [TestMethod]
        public async Task Chain_Catch()
        {
            var test = Chain
                .PostToChain()
                .Select(m => m.Text)
                .Where(a => false)
                .Catch<string, OperationCanceledException>((antecedent, error) => Chain.Return("world"))
                .PostToUser();

            using (var container = Build(Options.ResolveDialogFromContainer, test))
            using (var scope = container.BeginLifetimeScope(
                builder => builder.RegisterInstance(test).As<IDialog<object>>()))
            {
                await AssertScriptAsync(scope,
                    "hello",
                    "world"
                    );
            }
        }

        [TestMethod]
        public async Task SampleChain_Quiz()
        {
            var quiz = Chain
                .PostToChain()
                .Select(_ => "how many questions?")
                .PostToUser()
                .WaitToBot()
                .Select(m => int.Parse(m.Text))
                .Select(count => Enumerable.Range(0, count).Select(index => Chain.Return($"question {index + 1}?").PostToUser().WaitToBot().Select(m => m.Text)))
                .Fold((l, r) => l + "," + r)
                .Select(answers => "your answers were: " + answers)
                .PostToUser();

            using (var container = Build(Options.ResolveDialogFromContainer))
            using (var scope = container.BeginLifetimeScope(
                builder => builder.RegisterInstance(quiz).As<IDialog<object>>()))
            {
                await AssertScriptAsync(scope,
                    "hello",
                    "how many questions?",
                    "3",
                    "question 1?",
                    "A",
                    "question 2?",
                    "B",
                    "question 3?",
                    "C",
                    "your answers were: A,B,C"
                    );
            }
        }

        [TestMethod]
        public async Task SampleChain_While_Count()
        {
            var root =
                Chain
                .PostToChain()
                .Select(_ => (IReadOnlyList<string>)Array.Empty<string>())
                .While
                (
                    items => Chain
                                .Return(items)
                                .Select(i => i.Count < 3),
                    items => Chain
                                .Return(items)
                                .Select(i => $"question {i.Count}")
                                .PostToUser()
                                .WaitToBot()
                                .Select(a => items.Concat(new[] { a.Text }).ToArray())
                )
                .Select(items => string.Join(",", items))
                .PostToUser();

            using (var container = Build(Options.ResolveDialogFromContainer | Options.Reflection))
            using (var scope = container.BeginLifetimeScope(
                builder => builder.RegisterInstance(root).As<IDialog<object>>()))
            {
                await AssertScriptAsync(scope,
                    "hello",
                    "question 0",
                    "A",
                    "question 1",
                    "B",
                    "question 2",
                    "C",
                    "A,B,C"
                    );
            }
        }

        [TestMethod]
        public async Task SampleChain_Email()
        {
            Func<string, IDialog<string>> Ask = toUser =>
                Chain.Return(toUser)
                .PostToUser()
                .WaitToBot()
                .Select(m => m.Text);

            IDialog<IReadOnlyList<string>> recipientsDialog =
                Chain
                .Return(Array.Empty<string>())
                .While(items => Ask($"have {items.Length} recipients, want more?").Select(text => text == "yes"),
                items => Ask("next recipient?").Select(item => items.Concat(new[] { item }).ToArray()));

            var emailDialog = from hello in Chain.PostToChain().Select(m => m.Text + " back!").PostToUser()
                              from subject in Ask("what is the subject?")
                              from body in Ask("what is the body?")
                              from recipients in recipientsDialog
                              select new { subject, body, recipients };

            var rootDialog = emailDialog
                .Select(email => $"'{email.subject}': '{email.body}' to {email.recipients.Count} recipients")
                .PostToUser();

            using (var container = Build(Options.ResolveDialogFromContainer | Options.Reflection))
            using (var scope = container.BeginLifetimeScope(
                builder => builder.RegisterInstance(rootDialog).As<IDialog<object>>()))
            {
                await AssertScriptAsync(scope,
                    "hello",
                    "hello back!",
                    "what is the subject?",
                    "subject X",
                    "what is the body?",
                    "body Y",
                    "have 0 recipients, want more?",
                    "yes",
                    "next recipient?",
                    "person A",
                    "have 1 recipients, want more?",
                    "yes",
                    "next recipient?",
                    "person B",
                    "have 2 recipients, want more?",
                    "no",
                    "'subject X': 'body Y' to 2 recipients"
                    );
            }
        }

        [TestMethod]
        public async Task SampleChain_Joke()
        {
            var joke = Chain
                .PostToChain()
                .Select(m => m.Text)
                .Switch
                (
                    Chain.Case
                    (
                        new Regex("^chicken"),
                        (context, text) =>
                            Chain
                            .Return("why did the chicken cross the road?")
                            .PostToUser()
                            .WaitToBot()
                            .Select(ignoreUser => "to get to the other side")
                    ),
                    Chain.Default<string, IDialog<string>>(
                        (context, text) =>
                            Chain
                            .Return("why don't you like chicken jokes?")
                    )
                )
                .Unwrap()
                .PostToUser().
                Loop();

            using (var container = Build(Options.ResolveDialogFromContainer))
            using (var scope = container.BeginLifetimeScope(
                builder => builder.RegisterInstance(joke).As<IDialog<object>>()))
            {
                await AssertScriptAsync(scope,
                    "chicken",
                    "why did the chicken cross the road?",
                    "i don't know",
                    "to get to the other side",
                    "anything but chickens",
                    "why don't you like chicken jokes?"
                    );
            }
        }

        [TestMethod]
        public async Task SampleChain_Waterfall()
        {
            var waterfall = Chain
                .PostToChain()
                .ContinueWith(async (context, res) =>
                {
                    var msg = await res;
                    await context.PostAsync($"you said {msg.Text}");
                    return Chain.From(() => new PromptDialog.PromptChoice<string>(new[] { "a", "b", "c" }, "Which one you want to select?", string.Empty, 1, PromptStyle.None));
                })
                .ContinueWith(async (context, res) =>
                {
                    var selection = await res;
                    context.ConversationData.SetValue("selected", selection);
                    return (IDialog<bool>)new PromptDialog.PromptConfirm($"do you want {selection}?", string.Empty, 1, PromptStyle.None);
                })
                .Then(async (context, res) =>
                {

                    var selection = context.ConversationData.GetValue<string>("selected");
                    if (await res)
                    {
                        return $"{selection} is selected!";
                    }
                    else
                    {
                        return "selection canceled!";
                    }
                }).PostToUser();

            using (var container = Build(Options.ResolveDialogFromContainer))
            using (var scope = container.BeginLifetimeScope(
                builder => builder.RegisterInstance(waterfall).As<IDialog<object>>()))
            {
                await AssertScriptAsync(scope,
                    "test",
                    "you said test",
                    "Which one you want to select?",
                    "a",
                    "do you want a?",
                    "yes",
                    "a is selected!"
                    );
            }
        }
    }
}