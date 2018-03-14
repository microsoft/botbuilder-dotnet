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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Classic.Base;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.Dialogs.Internals
{
    /// <summary>
    /// A dialog task is a
    /// 1. single <see cref="IDialogStack"/> stack of <see cref="IDialog"/> frames, waiting on the next <see cref="IActivity"/>
    /// 2. the <see cref="IEventProducer{Activity}"/> queue of activity events necessary to satisfy those waits
    /// 2. the <see cref="IEventLoop"/> loop to execute that dialog code once the waits are satisfied
    /// </summary>
    public sealed class DialogTask : IDialogTask
    {
        private readonly Func<CancellationToken, IDialogContext> makeContext;
        private readonly IStore<IFiberLoop<DialogTask>> store;
        private readonly IEventProducer<IActivity> queue;
        private readonly IFiberLoop<DialogTask> fiber;
        private readonly Frames frames;
        public DialogTask(Func<CancellationToken, IDialogContext> makeContext, IStore<IFiberLoop<DialogTask>> store, IEventProducer<IActivity> queue)
        {
            SetField.NotNull(out this.makeContext, nameof(makeContext), makeContext);
            SetField.NotNull(out this.store, nameof(store), store);
            SetField.NotNull(out this.queue, nameof(queue), queue);
            this.store.TryLoad(out this.fiber);
            this.frames = new Frames(this);
        }

        private IWait<DialogTask> nextWait;
        private IWait<DialogTask> NextWait()
        {
            if (this.fiber.Frames.Count > 0)
            {
                var nextFrame = this.fiber.Frames.Peek();

                switch (nextFrame.Wait.Need)
                {
                    case Need.Wait:
                        // since the leaf frame is waiting, save this wait as the mark for that frame
                        nextFrame.Mark = nextFrame.Wait.CloneTyped();
                        break;
                    case Need.Call:
                        // because the user did not specify a new wait for the leaf frame during the call,
                        // reuse the previous mark for this frame
                        this.nextWait = nextFrame.Wait = nextFrame.Mark.CloneTyped();
                        break;
                    case Need.None:
                    case Need.Poll:
                        break;
                    case Need.Done:
                    default:
                        throw new NotImplementedException();
                }
            }

            return this.nextWait;
        }

        /// <summary>
        /// Adjust the calling convention from Dialog's to Fiber's delegates.
        /// </summary>
        /// <remarks>
        /// https://en.wikipedia.org/wiki/Thunk
        /// </remarks>
        public interface IThunk
        {
            Delegate Method { get; }
        }

        /// <summary>
        /// Adjust the calling convention from Dialog's to Fiber's delegates
        /// for IDialog.StartAsync.
        /// </summary>
        [Serializable]
        private sealed class ThunkStart : IThunk
        {
            private readonly StartAsync start;
            public ThunkStart(StartAsync start)
            {
                SetField.NotNull(out this.start, nameof(start), start);
            }

            public override string ToString()
            {
                return $"{this.start.Target}.{this.start.Method.Name}";
            }

            Delegate IThunk.Method => this.start;

            public async Task<IWait<DialogTask>> Rest(IFiber<DialogTask> fiber, DialogTask task, IItem<object> item, CancellationToken token)
            {
                var result = await item;
                if (result != null)
                {
                    throw new ArgumentException(nameof(item));
                }

                await this.start(task.makeContext(token));
                return task.NextWait();
            }
        }

        /// <summary>
        /// Adjust the calling convention from Dialog's to Fiber's delegates
        /// for IDialog's <see cref="ResumeAfter{T}"/>. 
        /// </summary>
        [Serializable]
        private sealed class ThunkResume<T> : IThunk
        {
            private readonly ResumeAfter<T> resume;
            public ThunkResume(ResumeAfter<T> resume)
            {
                SetField.NotNull(out this.resume, nameof(resume), resume);
            }

            public override string ToString()
            {
                return $"{this.resume.Target}.{this.resume.Method.Name}";
            }

            Delegate IThunk.Method => this.resume;

            public async Task<IWait<DialogTask>> Rest(IFiber<DialogTask> fiber, DialogTask task, IItem<T> item, CancellationToken token)
            {
                await this.resume(task.makeContext(token), item);
                return task.NextWait();
            }
        }

        internal Rest<DialogTask, object> ToRest(StartAsync start)
        {
            var thunk = new ThunkStart(start);
            return thunk.Rest;
        }

        internal Rest<DialogTask, T> ToRest<T>(ResumeAfter<T> resume)
        {
            var thunk = new ThunkResume<T>(resume);
            return thunk.Rest;
        }

        private sealed class Frames : IReadOnlyList<Delegate>
        {
            private readonly DialogTask task;
            public Frames(DialogTask task)
            {
                SetField.NotNull(out this.task, nameof(task), task);
            }

            int IReadOnlyCollection<Delegate>.Count
            {
                get
                {
                    return this.task.fiber.Frames.Count;
                }
            }

            public Delegate Map(int ordinal)
            {
                var frames = this.task.fiber.Frames;
                int index = frames.Count - ordinal - 1;
                var frame = frames[index];
                var wait = frame.Wait;
                var rest = wait.Rest;
                var thunk = (IThunk)rest.Target;
                return thunk.Method;
            }

            Delegate IReadOnlyList<Delegate>.this[int index]
            {
                get
                {
                    return this.Map(index);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                IEnumerable<Delegate> enumerable = this;
                return enumerable.GetEnumerator();
            }

            IEnumerator<Delegate> IEnumerable<Delegate>.GetEnumerator()
            {
                var frames = this.task.fiber.Frames;
                for (int index = 0; index < frames.Count; ++index)
                {
                    yield return this.Map(index);
                }
            }
        }

        IReadOnlyList<Delegate> IDialogStack.Frames
        {
            get
            {
                return this.frames;
            }
        }

        void IDialogStack.Call<R>(IDialog<R> child, ResumeAfter<R> resume)
        {
            var callRest = ToRest(child.StartAsync);
            if (resume != null)
            {
                var doneRest = ToRest(resume);
                this.nextWait = this.fiber.Call<DialogTask, object, R>(callRest, null, doneRest);
            }
            else
            {
                this.nextWait = this.fiber.Call<DialogTask, object>(callRest, null);
            }
        }

        async Task IDialogStack.Forward<R, T>(IDialog<R> child, ResumeAfter<R> resume, T item, CancellationToken token)
        {
            // put the child on the stack
            IDialogStack stack = this;
            stack.Call(child, resume);
            // run the loop
            IEventLoop loop = this;
            await loop.PollAsync(token);
            // forward the item
            this.fiber.Post(item);
            // run the loop again
            await loop.PollAsync(token);
        }

        void IDialogStack.Done<R>(R value)
        {
            this.nextWait = this.fiber.Done(value);
        }

        void IDialogStack.Fail(Exception error)
        {
            this.nextWait = this.fiber.Fail(error);
        }

        void IDialogStack.Wait<R>(ResumeAfter<R> resume)
        {
            this.nextWait = this.fiber.Wait<DialogTask, R>(ToRest(resume));
        }

        void IDialogStack.Post<E>(E @event, ResumeAfter<E> resume)
        {
            // schedule the wait for event delivery
            this.nextWait = this.fiber.Wait<DialogTask, E>(ToRest(resume));

            // save the wait for this event, in case the scorable action event handlers manipulate the stack
            var wait = this.nextWait;
            Action onPull = () =>
            {
                // and satisfy that particular saved wait when the event has been pulled from the queue
                wait.Post(@event);
            };

            // post the activity to the queue
            var activity = new Activity(ActivityTypes.Event) { Value = @event };
            this.queue.Post(activity, onPull);
        }

        void IDialogStack.Reset()
        {
            this.store.Reset();
            this.store.Flush();
            this.fiber.Reset();
        }

        async Task IEventLoop.PollAsync(CancellationToken token)
        {
            try
            {
                await this.fiber.PollAsync(this, token);

                // this line will throw an error if the code does not schedule the next callback
                // to wait for the next message sent from the user to the bot.
                this.fiber.Wait.ValidateNeed(Need.Wait);
            }
            catch
            {
                this.store.Reset();
                throw;
            }
            finally
            {
                this.store.Save(this.fiber);
                this.store.Flush();
            }
        }

        void IEventProducer<IActivity>.Post(IActivity item, Action onPull)
        {
            this.fiber.Post(item);
            onPull?.Invoke();
        }

        internal IStore<IFiberLoop<DialogTask>> Store
        {
            get
            {
                return this.store;
            }
        }
    }

    /// <summary>
    /// A reactive dialog task (in contrast to a proactive dialog task) is a dialog task that
    /// starts some root dialog when it receives the first <see cref="IActivity"/> activity. 
    /// </summary>
    public sealed class ReactiveDialogTask : IEventLoop, IEventProducer<IActivity>
    {
        private readonly IDialogTask dialogTask;
        private readonly Func<IDialog<object>> makeRoot;

        public ReactiveDialogTask(IDialogTask dialogTask, Func<IDialog<object>> makeRoot)
        {
            SetField.NotNull(out this.dialogTask, nameof(dialogTask), dialogTask);
            SetField.NotNull(out this.makeRoot, nameof(makeRoot), makeRoot);
        }

        async Task IEventLoop.PollAsync(CancellationToken token)
        {
            try
            {
                if (this.dialogTask.Frames.Count == 0)
                {
                    var root = this.makeRoot();
                    var loop = root.Loop();
                    this.dialogTask.Call(loop, null);
                }

                await this.dialogTask.PollAsync(token);
            }
            catch
            {
                this.dialogTask.Reset();
                throw;
            }
        }

        void IEventProducer<IActivity>.Post(IActivity item, Action onPull)
        {
            this.dialogTask.Post(item, onPull);
        }
    }

    /// <summary>
    /// This dialog task translates from the more orthogonal (opaque) fiber exceptions
    /// to the more readable dialog programming model exceptions.
    /// </summary>
    public sealed class ExceptionTranslationDialogTask : IPostToBot
    {
        private readonly IPostToBot inner;

        public ExceptionTranslationDialogTask(IPostToBot inner)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }

        async Task IPostToBot.PostAsync(IActivity activity, CancellationToken token)
        {
            try
            {
                await this.inner.PostAsync(activity, token);
            }
            catch (InvalidNeedException error) when (error.Need == Need.Wait && error.Have == Need.None)
            {
                throw new NoResumeHandlerException(error);
            }
            catch (InvalidNeedException error) when (error.Need == Need.Wait && error.Have == Need.Done)
            {
                throw new NoResumeHandlerException(error);
            }
            catch (InvalidNeedException error) when (error.Need == Need.Call && error.Have == Need.Wait)
            {
                throw new MultipleResumeHandlerException(error);
            }
        }
    }

    public sealed class EventLoopDialogTask : IPostToBot
    {
        private readonly Lazy<IEventLoop> inner;
        private readonly IEventProducer<IActivity> queue;
        public EventLoopDialogTask(Func<IEventLoop> makeInner, IEventProducer<IActivity> queue, IBotData botData)
        {
            SetField.NotNull(out this.queue, nameof(queue), queue);
            SetField.CheckNull(nameof(makeInner), makeInner);
            this.inner = new Lazy<IEventLoop>(() => makeInner());
        }

        async Task IPostToBot.PostAsync(IActivity activity, CancellationToken token)
        {
            this.queue.Post(activity);
            var loop = this.inner.Value;
            await loop.PollAsync(token);
        }
    }


    public sealed class QueueDrainingDialogTask : IPostToBot
    {
        private readonly IPostToBot inner;
        private readonly IBotToUser botToUser;
        private readonly IMessageQueue messageQueue;

        public QueueDrainingDialogTask(IPostToBot inner, IBotToUser botToUser, IMessageQueue messageQueue)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.botToUser, nameof(botToUser), botToUser);
            SetField.NotNull(out this.messageQueue, nameof(messageQueue), messageQueue);
        }

        async Task IPostToBot.PostAsync(IActivity activity, CancellationToken token)
        {
            await this.inner.PostAsync(activity, token);
            await this.messageQueue.DrainQueueAsync(this.botToUser, token);
        }
    }


    /// <summary>
    /// This dialog task loads the dialog stack from <see cref="IBotData"/> before handling the incoming
    /// activity and saves the dialog stack to <see cref="IBotData"/> afterwards. 
    /// </summary>
    public sealed class PersistentDialogTask : IPostToBot
    {
        private readonly IPostToBot inner;
        private readonly IBotData botData;

        public PersistentDialogTask(IPostToBot inner, IBotData botData)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.botData, nameof(botData), botData);
        }

        async Task IPostToBot.PostAsync(IActivity activity, CancellationToken token)
        {
            await botData.LoadAsync(token);
            try
            {
                await this.inner.PostAsync(activity, token);
            }
            finally
            {
                await botData.FlushAsync(token);
            }
        }
    }
}
