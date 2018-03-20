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

using Microsoft.Bot.Builder.Classic.Base;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Dialogs.Internals
{
    /// <summary>
    /// The stack of dialogs in the conversational process.
    /// </summary>
    public interface IDialogStack
    {
        /// <summary>
        /// The dialog frames active on the stack.
        /// </summary>
        IReadOnlyList<Delegate> Frames { get; }

        /// <summary>
        /// Suspend the current dialog until an external event has been sent to the bot.
        /// </summary>
        /// <param name="resume">The method to resume when the event has been received.</param>
        void Wait<R>(ResumeAfter<R> resume);

        /// <summary>
        /// Call a child dialog and add it to the top of the stack.
        /// </summary>
        /// <typeparam name="R">The type of result expected from the child dialog.</typeparam>
        /// <param name="child">The child dialog.</param>
        /// <param name="resume">The method to resume when the child dialog has completed.</param>
        void Call<R>(IDialog<R> child, ResumeAfter<R> resume);

        /// <summary>
        /// Post an internal event to the queue.
        /// </summary>
        /// <param name="event">The event to post to the queue.</param>
        /// <param name="resume">The method to resume when the event has been delivered.</param>
        void Post<E>(E @event, ResumeAfter<E> resume);

        /// <summary>
        /// Call a child dialog, add it to the top of the stack and post the item to the child dialog.
        /// </summary>
        /// <typeparam name="R">The type of result expected from the child dialog.</typeparam>
        /// <typeparam name="T">The type of the item posted to child dialog.</typeparam>
        /// <param name="child">The child dialog.</param>
        /// <param name="resume">The method to resume when the child dialog has completed.</param>
        /// <param name="item">The item that will be posted to child dialog.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A task representing the Forward operation.</returns>
        Task Forward<R, T>(IDialog<R> child, ResumeAfter<R> resume, T item, CancellationToken token);

        /// <summary>
        /// Complete the current dialog and return a result to the parent dialog.
        /// </summary>
        /// <typeparam name="R">The type of the result dialog.</typeparam>
        /// <param name="value">The value of the result.</param>
        void Done<R>(R value);

        /// <summary>
        /// Fail the current dialog and return an exception to the parent dialog.
        /// </summary>
        /// <param name="error">The error.</param>
        void Fail(Exception error);

        /// <summary>
        /// Resets the stack.
        /// </summary>
        void Reset();
    }

    public interface IDialogTask : IDialogStack, IEventLoop, IEventProducer<IActivity>
    {
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Interrupt the waiting dialog with a new dialog
        /// </summary>
        /// <typeparam name="T">The type of result expected from the dialog.</typeparam>
        /// <typeparam name="R">The type of the item posted to dialog.</typeparam>
        /// <param name="task">The dialog task.</param>
        /// <param name="dialog">The new interrupting dialog.</param>
        /// <param name="item">The item to forward to the new interrupting dialog.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A task that represents the interruption operation.</returns>
        public static async Task InterruptAsync<T, R>(this IDialogTask task, IDialog<T> dialog, R item, CancellationToken token)
        {
            await task.Forward(dialog.Void<T, R>(), null, item, token);
            await task.PollAsync(token);
        }
    }
}