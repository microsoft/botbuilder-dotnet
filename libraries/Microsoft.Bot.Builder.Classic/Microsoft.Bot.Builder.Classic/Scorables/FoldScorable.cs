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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Scorables.Internals
{
    /// <summary>
    /// The stage of the FoldScorable events.
    /// </summary>
    public enum FoldStage
    {
        /// <summary>
        /// After IScorable.PrepareAsync has been called and the state and score will be folded into the aggregated scorable.
        /// </summary>
        AfterFold,

        /// <summary>
        /// Before IScorable.PostAsync has been called to initiate the next best scorable's action.
        /// </summary>
        StartPost,

        /// <summary>
        /// After IScorable.PostAsync has been called to complete the next best scorable's action.
        /// </summary>
        AfterPost
    }

    /// <summary>
    /// Fold an aggregation of scorables to produce a winning scorable.
    /// </summary>
    /// <remarks>
    /// Fold aka "reduce, accumulate, aggregate, compress, or inject"
    /// https://en.wikipedia.org/wiki/Fold_(higher-order_function)
    /// </remarks>
    public abstract class FoldScorable<Item, Score> : ScorableBase<Item, IReadOnlyList<FoldScorable<Item, Score>.State>, Score>
    {
        /// <summary>
        /// Event handler delegate for fold scorable stages.
        /// </summary>
        public delegate bool OnStageDelegate(FoldStage stage, IScorable<Item, Score> scorable, Item item, object state, Score score);

        protected readonly IComparer<Score> comparer;
        protected readonly IEnumerable<IScorable<Item, Score>> scorables;

        public FoldScorable(IComparer<Score> comparer, IEnumerable<IScorable<Item, Score>> scorables)
        {
            SetField.NotNull(out this.comparer, nameof(comparer), comparer);
            SetField.NotNull(out this.scorables, nameof(scorables), scorables);
        }

        /// <summary>
        /// Event handler for fold scorable stages.
        /// </summary>
        /// <remarks>
        /// This is late-bound to allow binding to "this" in derived classes.
        /// </remarks>
        protected abstract OnStageDelegate OnStage { get; }

        /// <summary>
        /// Per-scorable opaque state used during scoring process.
        /// </summary>
        public struct State
        {
            public readonly int ordinal;
            public readonly IScorable<Item, Score> scorable;
            public readonly object state;
            public State(int ordinal, IScorable<Item, Score> scorable, object state)
            {
                this.ordinal = ordinal;
                this.scorable = scorable;
                this.state = state;
            }
        }

        protected override async Task<IReadOnlyList<State>> PrepareAsync(Item item, CancellationToken token)
        {
            var states = new List<State>();

            foreach (var scorable in this.scorables)
            {
                var state = await scorable.PrepareAsync(item, token);
                int ordinal = states.Count;
                states.Add(new State(ordinal, scorable, state));
                if (scorable.HasScore(item, state))
                {
                    var score = scorable.GetScore(item, state);
                    if (!this.OnStage(FoldStage.AfterFold, scorable, item, state, score))
                    {
                        break;
                    }
                }
            }

            states.Sort((one, two) =>
            {
                var oneHasScore = one.scorable.HasScore(item, one.state);
                var twoHasScore = two.scorable.HasScore(item, two.state);
                if (oneHasScore && twoHasScore)
                {
                    var oneScore = one.scorable.GetScore(item, one.state);
                    var twoScore = two.scorable.GetScore(item, two.state);

                    // sort largest scores first
                    var compare = this.comparer.Compare(twoScore, oneScore);
                    if (compare != 0)
                    {
                        return compare;
                    }
                }
                else if (oneHasScore)
                {
                    return -1;
                }
                else if (twoHasScore)
                {
                    return +1;
                }

                // stable sort otherwise
                return one.ordinal.CompareTo(two.ordinal);
            });

            return states;
        }

        protected override bool HasScore(Item item, IReadOnlyList<State> states)
        {
            if (states.Count > 0)
            {
                var state = states[0];
                return state.scorable.HasScore(item, state.state); 
            }

            return false;
        }

        protected override Score GetScore(Item item, IReadOnlyList<State> states)
        {
            var state = states[0];
            return state.scorable.GetScore(item, state.state);
        }

        protected override async Task PostAsync(Item item, IReadOnlyList<State> states, CancellationToken token)
        {
            foreach (var state in states)
            {
                if (!state.scorable.HasScore(item, state.state))
                {
                    break;
                }

                var score = state.scorable.GetScore(item, state.state);

                if (!this.OnStage(FoldStage.StartPost, state.scorable, item, state.state, score))
                {
                    break;
                }

                await state.scorable.PostAsync(item, state.state, token);

                if (!this.OnStage(FoldStage.AfterPost, state.scorable, item, state.state, score))
                {
                    break;
                }
            }
        }

        protected override async Task DoneAsync(Item item, IReadOnlyList<State> states, CancellationToken token)
        {
            foreach (var state in states)
            {
                await state.scorable.DoneAsync(item, state.state, token);
            }
        }
    }

    /// <summary>
    /// This scorable delegates the stage event handler to an external delegate or an overridable virtual method.
    /// </summary>
    public class DelegatingFoldScorable<Item, Score> : FoldScorable<Item, Score>
    {
        private readonly OnStageDelegate onStage;
        public DelegatingFoldScorable(OnStageDelegate onStage, IComparer<Score> comparer, IEnumerable<IScorable<Item, Score>> scorables)
            : base(comparer, scorables)
        {
            this.onStage = onStage ?? this.OnStageHandler;
        }

        protected override OnStageDelegate OnStage => this.onStage;

        public virtual bool OnStageHandler(FoldStage stage, IScorable<Item, Score> scorable, Item item, object state, Score score)
        {
            switch (stage)
            {
                case FoldStage.AfterFold: return true;
                case FoldStage.StartPost: return true;
                case FoldStage.AfterPost: return false;
                default: throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// A null comparer that pretends every item is equal.  This is particularly useful with stable sorts.
    /// </summary>
    public sealed class NullComparer<T> : IComparer<T>
    {
        public static readonly IComparer<T> Instance = new NullComparer<T>();

        private NullComparer()
        {
        }

        int IComparer<T>.Compare(T x, T y)
        {
            return 0;
        }
    }
}
