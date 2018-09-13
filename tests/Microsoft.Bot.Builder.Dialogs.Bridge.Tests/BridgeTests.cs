using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Dialogs.Bridge.Tests
{
    [TestClass]
    public class BridgeTests
    {
        public sealed class V4_Prompt : NumberPrompt<int>
        {
            public const string DialogId = nameof(V4_Prompt);
            public V4_Prompt() : base(DialogId)
            {
            }
        }

        public static async Task Send(IDialogContext context, string text)
        {
            var reply = context.MakeMessage();
            reply.Text = text;
            await context.PostAsync(reply, context.CancellationToken);
        }

        [Serializable]
        public abstract class V3_DialogBase : IDialog<string>
        {
            protected readonly string state;
            protected V3_DialogBase(string state)
            {
                this.state = state;
            }

            public abstract Task StartAsync(IDialogContext context);
            public string Text(object text, [CallerMemberName] string member = null)
                => $"{text} ({GetType().Name}:{this.state}:{member})";
        }

        [Serializable]
        public sealed class V3_to_V3_Dialog : V3_DialogBase
        {
            public V3_to_V3_Dialog(string state)
                : base(state)
            {
            }
            public override async Task StartAsync(IDialogContext context)
            {
                await Send(context, Text($"V3 start"));
                context.Call(new V3_to_V4_Dialog(state), AfterV3Call);
            }

            async Task AfterV3Call(IDialogContext context, IAwaitable<string> result)
            {
                var item = await result;
                await Send(context, Text($"V3 after '{item}'"));
                context.Done(item);
            }
        }


        [Serializable]
        public sealed class V3_to_V4_Dialog : V3_DialogBase
        {
            public V3_to_V4_Dialog(string state)
                : base(state)
            {
            }
            public override async Task StartAsync(IDialogContext context)
            {
                await Send(context, Text($"V3 start"));
                context.Wait(AfterV3Wait);
            }

            async Task AfterV3Wait(IDialogContext context, IAwaitable<IMessageActivity> result)
            {
                var message = await result;
                await Send(context, Text($"V3 after '{message.Text}'"));
                var options = new PromptOptions()
                {
                    Prompt = MessageFactory.Text("V4 prompt"),
                    RetryPrompt = MessageFactory.Text("V4 reprompt"),
                };
                await context.BeginAsync<int>(V4_Prompt.DialogId, options, AfterV4Wait);
            }

            async Task AfterV4Wait(IDialogContext context, IAwaitable<int> result)
            {
                var number = await result;
                await Send(context, Text(number));
                context.Done("done");
            }
        }

        [TestMethod]
        public async Task V4_Contains_V3_and_V4()
        {
            var convoState = new ConversationState(new MemoryStorage());

            var adapter = new TestAdapter()
                .Use(convoState);

            const string RootDialogId = "test";

            var dialogState = convoState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);
            dialogs.Add(new BridgeDialog());
            dialogs.Add(new V4_Prompt());
            dialogs.Add(new WaterfallDialog(RootDialogId, new WaterfallStep[]
            {
                    async (dc, step, token) => {
                        return await dc.BeginAsync(BridgeDialog.DialogId, Options.From(new V3_to_V3_Dialog("stateA")));
                    },
                    async (dc, step, token) => {
                        await dc.Context.SendActivityAsync(step.Result.ToString());
                        return await dc.BeginAsync(BridgeDialog.DialogId, Options.From(new V3_to_V3_Dialog("stateB")));
                    },
                    async (dc, step, token) => {
                        await dc.Context.SendActivityAsync(step.Result.ToString());
                        return Dialog.EndOfTurn;
                    }
            }));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext);
                await dc.ContinueAsync();
                if (!turnContext.Responded)
                {
                    await dc.BeginAsync(RootDialogId);
                }
            })
            .Send("hello")
            .AssertReply("V3 start (V3_to_V3_Dialog:stateA:StartAsync)")
            .AssertReply("V3 start (V3_to_V4_Dialog:stateA:StartAsync)")
            .Send("userA")
            .AssertReply("V3 after 'hello' (V3_to_V4_Dialog:stateA:AfterV3Wait)")
            .AssertReply("V4 prompt")
            .Send("NaN")
            .AssertReply("V4 reprompt")
            .Send("42")
            .AssertReply("V4 reprompt")
            .AssertReply("42 (V3_to_V4_Dialog:stateA:AfterV4Wait)")
            .AssertReply("V3 after 'done' (V3_to_V3_Dialog:stateA:AfterV3Call)")
            .AssertReply("done")
            .AssertReply("V3 start (V3_to_V3_Dialog:stateB:StartAsync)")
            .AssertReply("V3 start (V3_to_V4_Dialog:stateB:StartAsync)")
            .Send("userB")
            .AssertReply("V3 after '42' (V3_to_V4_Dialog:stateB:AfterV3Wait)")
            .AssertReply("V4 prompt")
            .Send("99")
            .AssertReply("V4 reprompt")
            .AssertReply("99 (V3_to_V4_Dialog:stateB:AfterV4Wait)")
            .AssertReply("V3 after 'done' (V3_to_V3_Dialog:stateB:AfterV3Call)")
            .AssertReply("done")
            .StartTestAsync();
        }
    }
}
