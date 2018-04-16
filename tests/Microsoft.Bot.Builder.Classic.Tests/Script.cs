using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    class Script : DialogTestBase
    {
        public static async Task RecordScript(ILifetimeScope container,
            bool proactive,
            StreamWriter stream,
            Func<string> extraInfo,
            params string[] inputs)
        {
            var toBot = MakeTestMessage();

            var adapter = new TestAdapter();
            await adapter.ProcessActivity((Activity)toBot, async (ctx) =>
            {
                using (var scope = DialogModule.BeginLifetimeScope(container, ctx))
                {
                    var task = scope.Resolve<IPostToBot>();
                    var queue = adapter.ActiveQueue;

                    Action drain = () =>
                    {
                        stream.WriteLine($"{queue.Count()}");
                        while (queue.Count > 0)
                        {
                            var toUser = queue.Dequeue();
                            if (!string.IsNullOrEmpty(toUser.Text))
                            {
                                stream.WriteLine($"ToUserText:{JsonConvert.SerializeObject(toUser.Text)}");
                            }
                            else
                            {
                                stream.WriteLine($"ToUserButtons:{JsonConvert.SerializeObject(toUser.Attachments)}");
                            }
                        }
                    };
                    string result = null;
                    var root = scope.Resolve<IDialog<object>>().Do(async (context, value) =>
                        result = JsonConvert.SerializeObject(await value));
                    if (proactive)
                    {
                        var loop = root.Loop();
                        var data = scope.Resolve<IBotData>();
                        await data.LoadAsync(CancellationToken.None);
                        var stack = scope.Resolve<IDialogTask>();
                        stack.Call(loop, null);
                        await stack.PollAsync(CancellationToken.None);
                        drain();
                    }
                    else
                    {
                        var builder = new ContainerBuilder();
                        builder
                            .RegisterInstance(root)
                            .AsSelf()
                            .As<IDialog<object>>();
                        builder.Update((IContainer)container);
                    }
                    foreach (var input in inputs)
                    {
                        stream.WriteLine($"FromUser:{JsonConvert.SerializeObject(input)}");
                        toBot.Text = input;
                        try
                        {
                            await task.PostAsync(toBot, CancellationToken.None);
                            drain();
                            if (extraInfo != null)
                            {
                                var extra = extraInfo();
                                stream.WriteLine(extra);
                            }
                        }
                        catch (Exception e)
                        {
                            stream.WriteLine($"Exception:{e.Message}");
                        }
                    }
                    if (result != null)
                    {
                        stream.WriteLine($"Result: {result}");
                    }
                }
            });
        }

        public static string ReadLine(StreamReader stream, out string label)
        {
            string line = stream.ReadLine();
            label = null;
            if (line != null)
            {
                int pos = line.IndexOf(':');
                if (pos != -1)
                {
                    label = line.Substring(0, pos);
                    line = line.Substring(pos + 1);
                }
            }
            return line;
        }

        public static async Task VerifyScript(ILifetimeScope container, Func<IDialog<object>> makeRoot, bool proactive, StreamReader stream, Action<IDialogStack, string> extraCheck, string[] expected, string locale)
        {
            var toBot = DialogTestBase.MakeTestMessage();
            if (!string.IsNullOrEmpty(locale))
            {
                toBot.Locale = locale;
            }

            string input, label;
            int current = 0;
            while ((input = ReadLine(stream, out label)) != null)
            {
                var adapter = new TestAdapter();
                await adapter.ProcessActivity((Activity)toBot, async (ctx) =>
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, ctx))
                    {
                        var task = scope.Resolve<IPostToBot>();
                        var queue = adapter.ActiveQueue;

                        Action<IDialogStack> check = (stack) =>
                        {
                            var count = int.Parse((proactive && current == 0) ? input : stream.ReadLine());
                            Assert.AreEqual(count, queue.Count);
                            for (var i = 0; i < count; ++i)
                            {
                                var toUser = queue.Dequeue();
                                var expectedOut = ReadLine(stream, out label);
                                if (label == "ToUserText")
                                {
                                    Assert.AreEqual(expectedOut, JsonConvert.SerializeObject(toUser.Text));
                                }
                                else
                                {
                                    Assert.AreEqual(expectedOut, JsonConvert.SerializeObject(toUser.Attachments));
                                }
                            }

                            extraCheck?.Invoke(stack, ReadLine(stream, out label));
                        };

                        Func<IDialog<object>> scriptMakeRoot = () =>
                        {
                            return makeRoot().Do(async (context, value) => context.PrivateConversationData.SetValue("result", JsonConvert.SerializeObject(await value)));
                        };
                        scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(scriptMakeRoot));

                        if (proactive && current == 0)
                        {
                            var loop = scriptMakeRoot().Loop();
                            var data = scope.Resolve<IBotData>();
                            await data.LoadAsync(CancellationToken.None);
                            var stack = scope.Resolve<IDialogTask>();
                            stack.Call(loop, null);
                            await stack.PollAsync(CancellationToken.None);
                            check(stack);
                            input = ReadLine(stream, out label);
                        }

                        if (input.StartsWith("\""))
                        {
                            try
                            {
                                toBot.Text = input.Substring(1, input.Length - 2);
                                Assert.IsTrue(current < expected.Length && toBot.Text == expected[current++]);
                                await task.PostAsync(toBot, CancellationToken.None);
                                var data = scope.Resolve<IBotData>();
                                await data.LoadAsync(CancellationToken.None);
                                var stack = scope.Resolve<IDialogStack>();
                                check(stack);
                            }
                            catch (Exception e)
                            {
                                Assert.AreEqual(ReadLine(stream, out label), e.Message);
                            }
                        }
                        else if (label.ToLower().StartsWith("result"))
                        {
                            var data = scope.Resolve<IBotData>();
                            await data.LoadAsync(CancellationToken.None);
                            string result;
                            Assert.IsTrue(data.PrivateConversationData.TryGetValue("result", out result));
                            Assert.AreEqual(input.Trim(), result);
                        }
                    }
                });
            }
        }

        public static async Task RecordDialogScript<T>(string filePath, IDialog<T> dialog, bool proactive, params string[] inputs)
        {
            using (var stream = new StreamWriter(filePath))
            using (var container = Build(Options.ResolveDialogFromContainer | Options.Reflection))
            using (var scope = container.BeginLifetimeScope(
                builder => builder.RegisterInstance(dialog).AsSelf().As<IDialog<object>>()))
            {
                await RecordScript(scope, proactive, stream, null, inputs);
            }
        }

        public static string NewScriptPathFor(string pathScriptOld)
        {
            var pathScriptNew = Path.Combine
                (
                Path.GetDirectoryName(pathScriptOld),
                Path.GetFileNameWithoutExtension(pathScriptOld) + "-new" + Path.GetExtension(pathScriptOld)
                );
            return pathScriptNew;
        }

        public static async Task VerifyDialogScript<T>(string filePath, IDialog<T> dialog, bool proactive, params string[] inputs)
        {
            var newPath = NewScriptPathFor(filePath);
            File.Delete(newPath);
            try
            {
                using (var stream = new StreamReader(filePath))
                using (var container = Build(Options.Reflection))
                {
                    await VerifyScript(container, () => (IDialog<object>)dialog, proactive, stream, null, inputs, locale: string.Empty);
                }
            }
            catch (Exception)
            {
                // There was an error, so record new script and pass on error
                await RecordDialogScript(newPath, dialog, proactive, inputs);
                throw;
            }
        }
    }
}
