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

using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Scorables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Scorables.Internals
{
    /// <summary>
    /// Allow for static type checking of opaque state for convenience of scorable implementations.
    /// </summary>
    /// <remarks>
    /// The IScorable methods are marked with DebuggerStepThrough because once the compiler has verified the type
    /// safety of the derived class that implements the abstract State-typed methods, these DebuggerStepThrough
    /// methods will not throw exceptions due to runtime type errors.
    /// </remarks>
    [Serializable]
    public abstract class ScorableBase<Item, State, Score> : IScorable<Item, Score>
    {
        protected abstract Task<State> PrepareAsync(Item item, CancellationToken token);

        protected abstract bool HasScore(Item item, State state);

        protected abstract Score GetScore(Item item, State state);

        protected abstract Task PostAsync(Item item, State state, CancellationToken token);

        protected abstract Task DoneAsync(Item item, State state, CancellationToken token);

        [DebuggerStepThrough]
        async Task<object> IScorable<Item, Score>.PrepareAsync(Item item, CancellationToken token)
        {
            return await this.PrepareAsync(item, token);
        }

        [DebuggerStepThrough]
        bool IScorable<Item, Score>.HasScore(Item item, object opaque)
        {
            var state = (State)opaque;
            return this.HasScore(item, state);
        }

        [DebuggerStepThrough]
        Score IScorable<Item, Score>.GetScore(Item item, object opaque)
        {
            var state = (State)opaque;
            if (!HasScore(item, state))
            {
                throw new InvalidOperationException();
            }

            return this.GetScore(item, state);
        }

        [DebuggerStepThrough]
        Task IScorable<Item, Score>.PostAsync(Item item, object opaque, CancellationToken token)
        {
            try
            {
                var state = (State)opaque;
                if (!HasScore(item, state))
                {
                    throw new InvalidOperationException();
                }

                return this.PostAsync(item, state, token);
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException(error);
            }
        }

        [DebuggerStepThrough]
        Task IScorable<Item, Score>.DoneAsync(Item item, object opaque, CancellationToken token)
        {
            try
            {
                var state = (State)opaque;
               
                return this.DoneAsync(item, state, token);
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException(error);
            }
        }
    }


    [Serializable]
    public abstract class DelegatingScorable<Item, Score> : IScorable<Item, Score>
    {
        protected readonly IScorable<Item, Score> inner;

        protected DelegatingScorable(IScorable<Item, Score> inner)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }

        public override string ToString()
        {
            return this.inner.ToString();
        }

        public virtual Task<object> PrepareAsync(Item item, CancellationToken token)
        {
            try
            {
                return this.inner.PrepareAsync(item, token);
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled<object>(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException<object>(error);
            }
        }

        public virtual bool HasScore(Item item, object state)
        {
            return this.inner.HasScore(item, state);
        }

        public virtual Score GetScore(Item item, object state)
        {
            return this.inner.GetScore(item, state);
        }

        public virtual Task PostAsync(Item item, object state, CancellationToken token)
        {
            try
            {
                return this.inner.PostAsync(item, state, token);
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException(error);
            }
        }

        public virtual Task DoneAsync(Item item, object state, CancellationToken token)
        {
            try
            {
                return this.inner.DoneAsync(item, state, token);
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException(error);
            }
        }
    }

    /// <summary>
    /// Provides the state to aggregate the state (and associated scorable) of multiple scorables.
    /// </summary>
    public class Token<InnerItem, InnerScore>
    {
        public InnerItem Item;
        public IScorable<InnerItem, InnerScore> Scorable;
        public object State;
    }

    /// <summary>
    /// Aggregates some non-empty set of inner scorables to produce an outer scorable.
    /// </summary>
    [Serializable]
    public abstract class ScorableAggregator<OuterItem, OuterState, OuterScore, InnerItem, InnerState, InnerScore> : ScorableBase<OuterItem, OuterState, OuterScore>
        where OuterState : Token<InnerItem, InnerScore>
    {
        protected override bool HasScore(OuterItem item, OuterState state)
        {
            if (state != null)
            {
                return state.Scorable.HasScore(state.Item, state.State);
            }

            return false;
        }

        protected override Task PostAsync(OuterItem item, OuterState state, CancellationToken token)
        {
            try
            {
                return state.Scorable.PostAsync(state.Item, state.State, token);
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException(error);
            }
        }

        protected override Task DoneAsync(OuterItem item, OuterState state, CancellationToken token)
        {
            try
            {
                if (state != null)
                {
                    return state.Scorable.DoneAsync(state.Item, state.State, token);
                }
                else
                {
                    return Task.CompletedTask;
                }
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException(error);
            }
        }
    }
}
