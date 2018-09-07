// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;

namespace Microsoft.Bot.Builder.Dialogs.Bridge
{
    public sealed class BridgeDialog : Dialog
    {
        public const string DialogId = nameof(BridgeDialog);
        public const string TurnStateKey = "ActivityConsumed";

        private readonly IFormatter _formatter;
        public BridgeDialog() : base(DialogId)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new FiberModule());
            builder
                .Register(c => new NetStandardSerialization.TypeSerializationSurrogate())
                .Keyed<Serialization.ISurrogateProvider>(FiberModule.Key_SurrogateProvider)
                .SingleInstance();
            builder
                .Register(c => new NetStandardSerialization.MemberInfoSerializationSurrogate())
                .Keyed<Serialization.ISurrogateProvider>(FiberModule.Key_SurrogateProvider)
                .SingleInstance();
            builder
                .Register(c => new NetStandardSerialization.DelegateSerializationSurrogate())
                .Keyed<Serialization.ISurrogateProvider>(FiberModule.Key_SurrogateProvider)
                .SingleInstance();
            builder
                .Register(c => new NetStandardSerialization.RegexSerializationSurrogate())
                .Keyed<Serialization.ISurrogateProvider>(FiberModule.Key_SurrogateProvider)
                .SingleInstance();

            using (var container = builder.Build())
            {
                _formatter = container.Resolve<IFormatter>();
            }            
        }

        public bool GetActivityConsumed(DialogContext dc)
        {
            var turn = dc.Context.TurnState;
            return turn.TryGetValue(TurnStateKey, out var value) && (bool)value == true;
        }

        public void SetActivityConsumed(DialogContext dc, bool consumed)
        {
            var turn = dc.Context.TurnState;
            turn[TurnStateKey] = consumed;
        }

        public override async Task<DialogTurnResult> DialogBeginAsync(DialogContext dc, DialogOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var instance = dc.ActiveDialog;
            Trace(dc.Context);
            var option = (Options)options;

            var dialog = option.Dialog;
            var context = new Context(dc);
            await option.StartAsync(context);
            // V4 seems to assume dialogs are started in response to incoming messages
            var result = await ToV4Async(dc, instance, context, dialog);

            bool consumed = GetActivityConsumed(dc);
            SetActivityConsumed(dc, true);

            if (result.Item1 == Action.Wait && !consumed)
            {
                return await DialogContinueAsync(dc);
            }
            else
            {
                return result.Item2;
            }
        }

        public override async Task<DialogTurnResult> DialogContinueAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            Trace(dc.Context);
            var instance = dc.ActiveDialog;
            var state = Load(_formatter, instance);
            SetActivityConsumed(dc, true);
            return await DialogNextAsync(dc, instance, state, dc.Context.Activity);
        }

        public override async Task<DialogTurnResult> DialogResumeAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Trace(dc.Context);
            var instance = dc.ActiveDialog;
            var state = Load(_formatter, instance);
            return await DialogNextAsync(dc, instance, state, result);
        }

        public override Task DialogEndAsync(ITurnContext context, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            Trace(context);
            return base.DialogEndAsync(context, instance, reason);
        }

        public override Task DialogRepromptAsync(ITurnContext context, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            Trace(context);
            return base.DialogRepromptAsync(context, instance);
        }
        public void Trace(ITurnContext context, [CallerMemberName] string member = null)
        {
            System.Diagnostics.Trace.TraceInformation($"{GetType().Name}.{member}: {context.Activity.Text}");
        }

        private async Task<DialogTurnResult> DialogNextAsync(DialogContext dc, DialogInstance instance, State state, object item)
        {
            var itemType = state.ItemType;
            var wrapType = typeof(AwaitableFromItem<>).MakeGenericType(itemType);
            var awaitable = Activator.CreateInstance(wrapType, item);

            var rest = state.Rest;
            var context = new Context(dc);
            var task = (Task)rest.Invoke(state.Dialog, new object[] { context, awaitable });
            await task;
            var result = await ToV4Async(dc, instance, context, state.Dialog);
            return result.Item2;
        }

        public enum Action { Call, Wait, Done }


        private async Task<Tuple<Action, DialogTurnResult>> ToV4Async(DialogContext dc, DialogInstance instance, Context context, object dialog)
        {
            var result = context.Result;
            var hasResult = context.HasResultValue;

            var state = new State() { Dialog = dialog };
            if (hasResult)
            {
                state.Rest = null;
            }
            else
            {
                state.Rest = context.Rest.Method ?? throw new ArgumentNullException(nameof(DialogNextAsync));
            }

            Save(_formatter, instance, state);

            if (context.Call != null)
            {
                return Tuple.Create(Action.Call, await dc.BeginAsync(BridgeDialog.DialogId, Options.From(context.Call)));
            }
            else if (hasResult)
            {
                return Tuple.Create(Action.Done, await dc.EndAsync(result.Result));
            }
            else
            {
                return Tuple.Create(Action.Wait, context.Result);
            }
        }

        private const string KeyBlob = "blob";

        private static void Save(IFormatter formatter, DialogInstance instance, State state)
        {
            using (var stream = new MemoryStream())
            using (var gzip = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true))
            {
                formatter.Serialize(gzip, state);
                var blob = stream.ToArray();
                var text = Convert.ToBase64String(blob);
                instance.State[KeyBlob] = text;
            }
        }

        private static State Load(IFormatter formatter, DialogInstance instance)
        {
            var text = (string)instance.State[KeyBlob];
            var blob = Convert.FromBase64String(text);
            using (var stream = new MemoryStream(blob))
            using (var gzip = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true))
            {
                var item = formatter.Deserialize(gzip);
                return (State)item;
            }
        }
    }
}
