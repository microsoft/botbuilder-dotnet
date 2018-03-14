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

using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Luis;
using Microsoft.Bot.Builder.Classic.Luis.Models;
using Microsoft.Bot.Builder.Classic.Scorables.Internals;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Scorables
{
    /// <summary>
    /// Fluent methods related to <see cref="IScorable{IResolver, Score}"/>.
    /// </summary>
    public static partial class Actions
    {
        private static IScorable<IResolver, IBinding> Bind(Delegate lambda)
        {
            return new DelegateScorable(lambda);
        }

        public static IScorable<IResolver, IBinding> Bind<R>(Func<R> method)
        {
            return Bind((Delegate)method);
        }

        public static IScorable<IResolver, IBinding> Bind<T1, R>(Func<T1, R> method)
        {
            return Bind((Delegate)method);
        }

        public static IScorable<IResolver, IBinding> Bind<T1, T2, R>(Func<T1, T2, R> method)
        {
            return Bind((Delegate)method);
        }

        public static IScorable<IResolver, IBinding> Bind<T1, T2, T3, R>(Func<T1, T2, T3, R> method)
        {
            return Bind((Delegate)method);
        }

        public static IScorable<IResolver, IBinding> Bind<T1, T2, T3, T4, R>(Func<T1, T2, T3, T4, R> method)
        {
            return Bind((Delegate)method);
        }

        public static IScorable<IResolver, IBinding> Bind<T1, T2, T3, T4, T5, R>(Func<T1, T2, T3, T4, T5, R> method)
        {
            return Bind((Delegate)method);
        }

        public static IScorable<IResolver, IBinding> Bind<T1, T2, T3, T4, T5, T6, R>(Func<T1, T2, T3, T4, T5, T6, R> method)
        {
            return Bind((Delegate)method);
        }

        public static IScorable<IResolver, IBinding> Bind<T1, T2, T3, T4, T5, T6, T7, R>(Func<T1, T2, T3, T4, T5, T6, T7, R> method)
        {
            return Bind((Delegate)method);
        }

        [Serializable]
        public sealed class WhereScorable<Score> : ScorableAggregator<IResolver, WhereScorable<Score>.Token, Score, IResolver, object, Score>
        {
            public sealed class Token : Token<IResolver, Score>
            {
                public bool HasScore { get; set; }
            }

            private readonly IScorable<IResolver, Score> scorable;
            private readonly Delegate lambda;
            public WhereScorable(IScorable<IResolver, Score> scorable, Delegate lambda)
            {
                SetField.NotNull(out this.scorable, nameof(scorable), scorable);
                SetField.NotNull(out this.lambda, nameof(lambda), lambda);
            }

            protected override async Task<Token> PrepareAsync(IResolver innerItem, CancellationToken token)
            {
                var innerState = await this.scorable.PrepareAsync(innerItem, token);
                bool hasScore = false;
                if (this.scorable.HasScore(innerItem, innerState))
                {
                    var innerScore = this.scorable.GetScore(innerItem, innerState);
                    var outerItem = new ArrayResolver(innerItem, innerScore);

                    IBinding<bool> binding;
                    if (Binder.Instance.TryBind<bool>(lambda, outerItem, out binding))
                    {
                        hasScore = await binding.InvokeAsync(token);
                    }
                }

                var state = new Token()
                {
                    Item = innerItem,
                    Scorable = this.scorable,
                    State = innerState,
                    HasScore = hasScore
                };

                return state;
            }

            protected override bool HasScore(IResolver item, Token state)
            {
                return state.HasScore;
            }

            protected override Score GetScore(IResolver item, Token state)
            {
                return state.Scorable.GetScore(state.Item, state.State);
            }
        }

        public static IScorable<IResolver, Score> Where<Score, T1>(this IScorable<IResolver, Score> scorable, Func<T1, bool> predicate)
        {
            return new WhereScorable<Score>(scorable, predicate);
        }

        public static IScorable<IResolver, Score> Where<Score, T1>(this IScorable<IResolver, Score> scorable, Func<T1, Task<bool>> predicate)
        {
            return new WhereScorable<Score>(scorable, predicate);
        }

        public static IScorable<IResolver, Score> Where<Score, T1, T2>(this IScorable<IResolver, Score> scorable, Func<T1, T2, bool> predicate)
        {
            return new WhereScorable<Score>(scorable, predicate);
        }

        public static IScorable<IResolver, Score> Where<Score, T1, T2>(this IScorable<IResolver, Score> scorable, Func<T1, T2, Task<bool>> predicate)
        {
            return new WhereScorable<Score>(scorable, predicate);
        }

        public static IScorable<IResolver, Score> Where<Score, T1, T2, T3>(this IScorable<IResolver, Score> scorable, Func<T1, T2, T3, bool> predicate)
        {
            return new WhereScorable<Score>(scorable, predicate);
        }

        public static IScorable<IResolver, Score> Where<Score, T1, T2, T3>(this IScorable<IResolver, Score> scorable, Func<T1, T2, T3, Task<bool>> predicate)
        {
            return new WhereScorable<Score>(scorable, predicate);
        }

        public static IScorable<IResolver, Match> When<InnerScore>(this IScorable<IResolver, InnerScore> scorable, Regex regex)
        {
            return new RegexMatchScorable<object, InnerScore>(regex, scorable);
        }

        public static IScorable<IResolver, IntentRecommendation> When<InnerScore>(this IScorable<IResolver, InnerScore> scorable, ILuisModel model, LuisIntentAttribute intent, ILuisService service = null)
        {
            service = service ?? new LuisService(model);
            return new LuisIntentScorable<object, InnerScore>(service, model, intent, scorable);
        }

        public static IScorable<IResolver, double> Normalize(this IScorable<IResolver, IBinding> scorable)
        {
            return scorable.SelectScore((r, b) => 1.0);
        }

        public static IScorable<IResolver, double> Normalize(this IScorable<IResolver, Match> scorable)
        {
            return scorable.SelectScore((r, m) => RegexMatchScorable.ScoreFor(m));
        }

        public static IScorable<IResolver, double> Normalize(this IScorable<IResolver, IntentRecommendation> scorable)
        {
            return scorable.SelectScore((r, i) => i.Score.GetValueOrDefault());
        }
    }
}