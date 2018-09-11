// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Bridge
{
    public sealed class Context : IDialogContext
    {
        private readonly DialogContext dc;
        public Context(DialogContext dc)
        {
            this.dc = dc ?? throw new ArgumentNullException(nameof(dc));
        }

        public DialogContext V4 => this.dc;
        public bool HasResultValue { get; private set; }
        public DialogTurnResult Result { get; } = new DialogTurnResult(DialogTurnStatus.Empty);
        public Delegate Rest { get; private set; }
        public IDialog<object> Call { get; private set; }

        IReadOnlyList<Delegate> IDialogStack.Frames => throw new NotImplementedException();
        CancellationToken IBotContext.CancellationToken => CancellationToken.None;
        IActivity IBotContext.Activity => dc.Context.Activity;
        IBotDataBag IBotData.UserData => throw new NotImplementedException();
        IBotDataBag IBotData.ConversationData => throw new NotImplementedException();
        IBotDataBag IBotData.PrivateConversationData => throw new NotImplementedException();

        IMessageActivity IBotToUser.MakeMessage()
        {
            return dc.Context.Activity.CreateReply();
        }
        void IDialogStack.Call<R>(IDialog<R> child, ResumeAfter<R> resume)
        {
            // TODO: handle value types
            Call = child as IDialog<object>;
            Rest = resume;

            HasResultValue = false;
            Result.Result = null;
        }

        void IDialogStack.Wait<R>(ResumeAfter<R> resume)
        {
            Rest = resume;

            HasResultValue = false;
            Result.Result = null;
        }

        void IDialogStack.Done<R>(R value)
        {
            HasResultValue = true;
            Result.Result = value;
        }

        async Task IBotToUser.PostAsync(IMessageActivity message, CancellationToken cancellationToken)
        {
            await this.dc.Context.SendActivityAsync(message, cancellationToken);
        }

        void IDialogStack.Fail(Exception error)
        {
            // TODO: does V4 propagate exceptions up dialog stack?
            throw new NotImplementedException();
        }

        Task IBotData.FlushAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        async Task IDialogStack.Forward<R, T>(IDialog<R> child, ResumeAfter<R> resume, T item, CancellationToken token)
        {
            Call = child as IDialog<object>;
            Rest = resume;

            HasResultValue = false;
            Result.Result = null;

            dc.SetActivityConsumed(false);
        }

        Task IBotData.LoadAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        void IDialogStack.Post<E>(E @event, ResumeAfter<E> resume)
        {
            throw new NotImplementedException();
        }
        void IDialogStack.Reset()
        {
            throw new NotImplementedException();
        }
    }
}
