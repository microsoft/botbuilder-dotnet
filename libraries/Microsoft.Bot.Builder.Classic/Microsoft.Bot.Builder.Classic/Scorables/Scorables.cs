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
using Microsoft.Bot.Builder.Classic.Scorables.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.Classic.Luis;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Luis.Models;

namespace Microsoft.Bot.Builder.Classic.Scorables
{
    public static partial class Scorable
    {
        /// <summary>
        /// Invoke the scorable calling protocol against a single scorable.
        /// </summary>
        public static async Task<bool> TryPostAsync<Item, Score>(this IScorable<Item, Score> scorable, Item item, CancellationToken token)
        {
            var state = await scorable.PrepareAsync(item, token);
            try
            {
                if (scorable.HasScore(item, state))
                {
                    var score = scorable.GetScore(item, state);
                    await scorable.PostAsync(item, state, token);
                    return true;
                }

                return false;
            }
            finally
            {
                await scorable.DoneAsync(item, state, token);
            }
        }

        public static IScorable<Item, Score> WhereScore<Item, Score>(this IScorable<Item, Score> scorable, Func<Item, Score, bool> predicate)
        {
            return new WhereScoreScorable<Item, Score>(scorable, predicate);
        }

        /// <summary>
        /// Project the score of a scorable using a lambda expression.
        /// </summary>
        public static IScorable<Item, TargetScore> SelectScore<Item, SourceScore, TargetScore>(this IScorable<Item, SourceScore> scorable, Func<Item, SourceScore, TargetScore> selector)
        {
            return new SelectScoreScorable<Item, SourceScore, TargetScore>(scorable, selector);
        }

        /// <summary>
        /// Project the item of a scorable using a lambda expression.
        /// </summary>
        public static IScorable<SourceItem, Score> SelectItem<SourceItem, TargetItem, Score>(this IScorable<TargetItem, Score> scorable, Func<SourceItem, TargetItem> selector)
        {
            return new SelectItemScorable<SourceItem, TargetItem, Score>(scorable, selector);
        }

        /// <summary>
        /// True if the scorable is non-null, false otherwise.
        /// </summary>
        public static bool Keep<Item, Score>(IScorable<Item, Score> scorable)
        {
            // complicated because IScorable<Item, Score> is variant on generic parameter,
            // so generic type arguments of NullScorable<,> may not match generic type
            // arguments of IScorable<,>
            var type = scorable.GetType();
            if (type.IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();
                if (typeof(NullScorable<,>).IsAssignableFrom(definition))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Try to simplify a list of scorables.
        /// </summary>
        /// <param name="scorables">The simplified list of scorables.</param>
        /// <param name="scorable">The single scorable representing the list, if possible.</param>
        /// <returns>True if it were possible to reduce the list of scorables to a single scorable, false otherwise.</returns>
        public static bool TryReduce<Item, Score>(ref IEnumerable<IScorable<Item, Score>> scorables, out IScorable<Item, Score> scorable)
        {
            // only if this is a fixed list of scorables, but never a lazy enumerable
            var list = scorables as IReadOnlyList<IScorable<Item, Score>>;
            if (list != null)
            {
                var itemCount = list.Count;
                int keepCount = 0;
                for (int index = 0; index < itemCount; ++index)
                {
                    var item = list[index];
                    if (Keep(item))
                    {
                        ++keepCount;
                    }
                }

                // empty non-null list is null scorable
                if (keepCount == 0)
                {
                    scorable = NullScorable<Item, Score>.Instance;
                    return true;
                }
                // single item non-null list is just that scorable
                else if (keepCount == 1)
                {
                    for (int index = 0; index < itemCount; ++index)
                    {
                        var item = list[index];
                        if (Keep(item))
                        {
                            scorable = item;
                            return true;
                        }
                    }
                }
                // non-null subset of that list is just those scorables
                else if (keepCount < itemCount)
                {
                    var keep = new IScorable<Item, Score>[keepCount];
                    int keepIndex = 0;
                    for (int index = 0; index < itemCount; ++index)
                    {
                        var item = list[index];
                        if (Keep(item))
                        {
                            keep[keepIndex] = item;
                            ++keepIndex;
                        }
                    }

                    scorables = keep;
                }
            }

            scorable = null;
            return false;
        }

        /// <summary>
        /// Select the first scorable that produces a score.
        /// </summary>
        public static IScorable<Item, Score> First<Item, Score>(this IEnumerable<IScorable<Item, Score>> scorables)
        {
            IScorable<Item, Score> scorable;
            if (TryReduce(ref scorables, out scorable))
            {
                return scorable;
            }

            return new FirstScorable<Item, Score>(scorables);
        }

        /// <summary>
        /// Fold an enumeration of scorables using a score comparer.
        /// </summary>
        public static IScorable<Item, Score> Fold<Item, Score>(this IEnumerable<IScorable<Item, Score>> scorables, IComparer<Score> comparer = null, FoldScorable<Item, Score>.OnStageDelegate onStage = null)
        {
            comparer = comparer ?? Comparer<Score>.Default;

            IScorable<Item, Score> scorable;
            if (TryReduce(ref scorables, out scorable))
            {
                return scorable;
            }

            return new DelegatingFoldScorable<Item, Score>(onStage, comparer, scorables);
        }
    }
}

namespace Microsoft.Bot.Builder.Classic.Scorables.Internals
{
    [Serializable]
    public sealed class NullScorable<Item, Score> : IScorable<Item, Score>
    {
        public static readonly IScorable<Item, Score> Instance = new NullScorable<Item, Score>();

        private NullScorable()
        {
        }

        Task<object> IScorable<Item, Score>.PrepareAsync(Item item, CancellationToken token)
        {
            return Tasks<object>.Null;
        }

        bool IScorable<Item, Score>.HasScore(Item item, object state)
        {
            return false;
        }

        Score IScorable<Item, Score>.GetScore(Item item, object state)
        {
            throw new NotImplementedException();
        }

        Task IScorable<Item, Score>.PostAsync(Item item, object state, CancellationToken token)
        {
            return Task.FromException(new NotImplementedException());
        }

        Task IScorable<Item, Score>.DoneAsync(Item item, object state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }

    [Serializable]
    public sealed class WhereScoreScorable<Item, Score> : DelegatingScorable<Item, Score>
    {
        private readonly Func<Item, Score, bool> predicate;

        public WhereScoreScorable(IScorable<Item, Score> scorable, Func<Item, Score, bool> predicate)
            : base(scorable)
        {
            SetField.NotNull(out this.predicate, nameof(predicate), predicate);
        }

        public override bool HasScore(Item item, object state)
        {
            if (base.HasScore(item, state))
            {
                var score = base.GetScore(item, state);
                if (this.predicate(item, score))
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public sealed class SelectItemScorable<OuterItem, InnerItem, Score> : ScorableAggregator<OuterItem, Token<InnerItem, Score>, Score, InnerItem, object, Score>
    {
        private readonly IScorable<InnerItem, Score> scorable;
        private readonly Func<OuterItem, InnerItem> selector;

        public SelectItemScorable(IScorable<InnerItem, Score> scorable, Func<OuterItem, InnerItem> selector)
        {
            SetField.NotNull(out this.scorable, nameof(scorable), scorable);
            SetField.NotNull(out this.selector, nameof(selector), selector);
        }

        protected override async Task<Token<InnerItem, Score>> PrepareAsync(OuterItem sourceItem, CancellationToken token)
        {
            var targetItem = this.selector(sourceItem);
            var state = new Token<InnerItem, Score>()
            {
                Item = targetItem,
                Scorable = this.scorable,
                State = await this.scorable.PrepareAsync(targetItem, token)
            };
            return state;
        }

        protected override Score GetScore(OuterItem item, Token<InnerItem, Score> state)
        {
            return state.Scorable.GetScore(state.Item, state.State);
        }
    }

    [Serializable]
    public sealed class SelectScoreScorable<Item, SourceScore, TargetScore> : DelegatingScorable<Item, SourceScore>, IScorable<Item, TargetScore>
    {
        private readonly Func<Item, SourceScore, TargetScore> selector;

        public SelectScoreScorable(IScorable<Item, SourceScore> scorable, Func<Item, SourceScore, TargetScore> selector)
            : base(scorable)
        {
            SetField.NotNull(out this.selector, nameof(selector), selector);
        }

        TargetScore IScorable<Item, TargetScore>.GetScore(Item item, object state)
        {
            IScorable<Item, SourceScore> source = this.inner;
            var sourceScore = source.GetScore(item, state);
            var targetScore = this.selector(item, sourceScore);
            return targetScore;
        }
    }

    public sealed class FirstScorable<Item, Score> : DelegatingFoldScorable<Item, Score>
    {
        public FirstScorable(IEnumerable<IScorable<Item, Score>> scorables)
            : base(null, Comparer<Score>.Default, scorables)
        {
        }

        public override bool OnStageHandler(FoldStage stage, IScorable<Item, Score> scorable, Item item, object state, Score score)
        {
            switch (stage)
            {
                case FoldStage.AfterFold: return false;
                case FoldStage.StartPost: return true;
                case FoldStage.AfterPost: return false;
                default: throw new NotImplementedException();
            }
        }
    }

    public sealed class TraitsScorable<Item, Score> : DelegatingFoldScorable<Item, Score>
    {
        private readonly ITraits<Score> traits;

        public TraitsScorable(ITraits<Score> traits, IComparer<Score> comparer, IEnumerable<IScorable<Item, Score>> scorables)
            : base(null, comparer, scorables)
        {
            SetField.NotNull(out this.traits, nameof(traits), traits);
        }

        public override bool OnStageHandler(FoldStage stage, IScorable<Item, Score> scorable, Item item, object state, Score score)
        {
            switch (stage)
            {
                case FoldStage.AfterFold: return OnFold(scorable, item, state, score);
                case FoldStage.StartPost: return true;
                case FoldStage.AfterPost: return false;
                default: throw new NotImplementedException();
            }
        }

        private bool OnFold(IScorable<Item, Score> scorable, Item item, object state, Score score)
        {
            if (this.comparer.Compare(score, this.traits.Minimum) < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(score));
            }

            var maximum = this.comparer.Compare(score, this.traits.Maximum);
            if (maximum > 0)
            {
                throw new ArgumentOutOfRangeException(nameof(score));
            }
            else if (maximum == 0)
            {
                return false;
            }

            return true;
        }
    }
}