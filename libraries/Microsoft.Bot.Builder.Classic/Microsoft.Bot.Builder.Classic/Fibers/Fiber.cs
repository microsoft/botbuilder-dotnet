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
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Internals.Fibers
{
    /// <summary>
    /// Waiters wait for an item to be posted.
    /// </summary>
    /// <remarks>
    /// Fibers and fiber frames are both waiters.
    /// </remarks>
    public interface IWaiter<C>
    {
        /// <summary>
        /// A "mailbox" for storing a wait associated with this frame.
        /// </summary>
        IWait<C> Mark { get; set; }

        /// <summary>
        /// The active wait for this waiter.
        /// </summary>
        IWait<C> Wait { get; set; }
    }

    public interface IFiber<C> : IWaiter<C>
    {
        IWaitFactory<C> Waits { get; }
        IReadOnlyList<IFrame<C>> Frames { get; }
        void Push();
        void Done();
    }

    public interface IFiberLoop<C> : IFiber<C>
    {
        Task<IWait<C>> PollAsync(C context, CancellationToken token);
    }

    public interface IFrameLoop<C>
    {
        Task<IWait<C>> PollAsync(IFiber<C> fiber, C context, CancellationToken token);
    }


    public interface IFrame<C> : IWaiter<C>, IFrameLoop<C>
    {
    }

    [Serializable]
    public sealed class Frame<C> : IFrame<C>
    {
        private IWait<C> mark = NullWait<C>.Instance;
        private IWait<C> wait = NullWait<C>.Instance;

        public override string ToString()
        {
            return this.wait.ToString();
        }

        IWait<C> IWaiter<C>.Mark
        {
            get { return this.mark; }
            set { this.mark = value; }
        }

        IWait<C> IWaiter<C>.Wait
        {
            get { return this.wait; }
            set
            {
                if (this.wait is NullWait<C>)
                {
                    this.wait = null;
                }

                if (this.wait != null)
                {
                    this.wait.ValidateNeed(Need.Call);
                    this.wait = null;
                }

                this.wait = value;
            }
        }

        async Task<IWait<C>> IFrameLoop<C>.PollAsync(IFiber<C> fiber, C context, CancellationToken token)
        {
            return await this.wait.PollAsync(fiber, context, token);
        }
    }

    public interface IFrameFactory<C>
    {
        IFrame<C> Make();
    }

    [Serializable]
    public sealed class FrameFactory<C> : IFrameFactory<C>
    {
        IFrame<C> IFrameFactory<C>.Make()
        {
            return new Frame<C>();
        }
    }

    [Serializable]
    public sealed class Fiber<C> : IFiber<C>, IFiberLoop<C>
    {
        private readonly List<IFrame<C>> stack = new List<IFrame<C>>();
        private readonly IFrameFactory<C> frames;
        private readonly IWaitFactory<C> waits;

        public Fiber(IFrameFactory<C> factory, IWaitFactory<C> waits)
        {
            SetField.NotNull(out this.frames, nameof(factory), factory);
            SetField.NotNull(out this.waits, nameof(waits), waits);
        }

        IWaitFactory<C> IFiber<C>.Waits => this.waits;

        IReadOnlyList<IFrame<C>> IFiber<C>.Frames => this.stack;

        void IFiber<C>.Push()
        {
            this.stack.Push(this.frames.Make());
        }

        void IFiber<C>.Done()
        {
            this.stack.Pop();
        }

        IWait<C> IWaiter<C>.Mark
        {
            get
            {
                if (this.stack.Count > 0)
                {
                    var leaf = this.stack.Peek();
                    return leaf.Mark;
                }
                else
                {
                    return NullWait<C>.Instance;
                }
            }
            set
            {
                this.stack.Peek().Mark = value;
            }
        }

        IWait<C> IWaiter<C>.Wait
        {
            get
            {
                if (this.stack.Count > 0)
                {
                    var leaf = this.stack.Peek();
                    return leaf.Wait;
                }
                else
                {
                    return NullWait<C>.Instance;
                }
            }
            set
            {
                this.stack.Peek().Wait = value;
            }
        }

        async Task<IWait<C>> IFiberLoop<C>.PollAsync(C context, CancellationToken token)
        {
            while (this.stack.Count > 0)
            {
                var leaf = this.stack.Peek();
                var wait = leaf.Wait;
                switch (wait.Need)
                {
                    case Need.None:
                    case Need.Wait:
                    case Need.Done:
                        return wait;
                    case Need.Poll:
                        break;
                    default:
                        throw new InvalidNeedException(wait, Need.Poll);
                }

                try
                {
                    var next = await leaf.PollAsync(this, context, token);
                    var peek = this.stack.Peek();
                    bool fine = object.ReferenceEquals(next, peek.Wait) || next is NullWait<C>;
                    if (!fine)
                    {
                        throw new InvalidNextException(next);
                    }
                }
                catch (Exception error)
                {
                    this.stack.Pop();
                    if (this.stack.Count == 0)
                    {
                        throw;
                    }
                    else
                    {
                        var parent = this.stack.Peek();
                        parent.Wait.Fail(error);
                    }
                }
            }

            return NullWait<C>.Instance;
        }
    }
}