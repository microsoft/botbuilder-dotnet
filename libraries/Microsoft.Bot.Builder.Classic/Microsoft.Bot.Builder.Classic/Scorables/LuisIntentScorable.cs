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
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Scorables.Internals
{
    public sealed class LuisIntentScorableFactory : IScorableFactory<IResolver, IntentRecommendation>
    {
        private readonly Func<ILuisModel, ILuisService> make;
        public LuisIntentScorableFactory(Func<ILuisModel, ILuisService> make)
        {
            SetField.NotNull(out this.make, nameof(make), make);
        }

        IScorable<IResolver, IntentRecommendation> IScorableFactory<IResolver, IntentRecommendation>.ScorableFor(IEnumerable<MethodInfo> methods)
        {
            var scorableByMethod = methods.ToDictionary(m => m, m => new MethodScorable(m));

            var specs =
                from method in methods
                from model in InheritedAttributes.For<LuisModelAttribute>(method)
                from intent in InheritedAttributes.For<LuisIntentAttribute>(method)
                select new { method, intent, model };

            // for a given LUIS model and intent, fold the corresponding method scorables together to enable overload resolution
            var scorables =
                from spec in specs
                group spec by new { spec.model, spec.intent } into modelIntents
                let method = modelIntents.Select(m => scorableByMethod[m.method]).ToArray().Fold(BindingComparer.Instance)
                let service = this.make(modelIntents.Key.model)
                select new LuisIntentScorable<IBinding, IBinding>(service, modelIntents.Key.model, modelIntents.Key.intent, method);

            var all = scorables.ToArray().Fold(IntentComparer.Instance);

            return all;
        }
    }

    /// <summary>
    /// Scorable to represent a specific LUIS intent recommendation.
    /// </summary>
    [Serializable]
    public sealed class LuisIntentScorable<InnerState, InnerScore> : ResolverScorable<LuisIntentScorable<InnerState, InnerScore>.Scope, IntentRecommendation, InnerState, InnerScore>
    {
        private readonly ILuisService service;
        private readonly ILuisModel model;
        private readonly LuisIntentAttribute intent;

        public sealed class Scope : ResolverScope<InnerScore>
        {
            public readonly ILuisModel Model;
            public readonly LuisResult Result;
            public readonly IntentRecommendation Intent;
            public Scope(ILuisModel model, LuisResult result, IntentRecommendation intent, IResolver inner)
                : base(inner)
            {
                SetField.NotNull(out this.Model, nameof(model), model);
                SetField.NotNull(out this.Result, nameof(result), result);
                SetField.NotNull(out this.Intent, nameof(intent), intent);
            }
            public override bool TryResolve(Type type, object tag, out object value)
            {
                if (type.IsAssignableFrom(typeof(ILuisModel)))
                {
                    value = this.Model;
                    return true;
                }
                if (type.IsAssignableFrom(typeof(LuisResult)))
                {
                    value = this.Result;
                    return true;
                }
                if (type.IsAssignableFrom(typeof(IntentRecommendation)))
                {
                    value = this.Intent;
                    return true;
                }

                var name = tag as string;
                if (name != null)
                {
                    var typeE = type.IsAssignableFrom(typeof(EntityModel));
                    var typeS = type.IsAssignableFrom(typeof(string));
                    var typeIE = type.IsAssignableFrom(typeof(IReadOnlyList<EntityModel>));
                    var typeIS = type.IsAssignableFrom(typeof(IReadOnlyList<string>));
                    if (typeE || typeS || typeIE || typeIS)
                    {
                        var entities = this.Result.Entities.Where(e => e.Type == name).ToArray();
                        if (entities.Length > 0)
                        {
                            if (entities.Length == 1)
                            {
                                if (typeE)
                                {
                                    value = entities[0];
                                    return true;
                                }
                                if (typeS)
                                {
                                    value = entities[0].Entity;
                                    return true;
                                }
                            }

                            if (typeIE)
                            {
                                value = entities;
                                return true;
                            }
                            if (typeIS)
                            {
                                value = entities.Select(e => e.Entity).ToArray();
                                return true;
                            }
                        }
                        // TODO: parsing and interpretation of LUIS entity resolutions
                    }
                }

                // i.e. for IActivity
                return base.TryResolve(type, tag, out value);
            }
        }

        public LuisIntentScorable(ILuisService service, ILuisModel model, LuisIntentAttribute intent, IScorable<IResolver, InnerScore> inner)
            : base(inner)
        {
            SetField.NotNull(out this.service, nameof(service), service);
            SetField.NotNull(out this.model, nameof(model), model);
            SetField.NotNull(out this.intent, nameof(intent), intent);
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.intent}, {this.inner})";
        }

        // assumes that LuisResult is cacheable with Uri as complete key (i.e. ILuisService is not required)
        private static readonly ConditionalWeakTable<IResolver, Dictionary<Uri, Task<LuisResult>>> Cache
            = new ConditionalWeakTable<IResolver, Dictionary<Uri, Task<LuisResult>>>();

        protected override async Task<Scope> PrepareAsync(IResolver resolver, CancellationToken token)
        {
            IMessageActivity message;
            if (!resolver.TryResolve(null, out message))
            {
                return null;
            }

            var text = message.Text;
            if (text == null)
            {
                return null;
            }

            var taskByUri = Cache.GetOrCreateValue(resolver);

            var uri = this.service.BuildUri(text);
            Task<LuisResult> task;
            lock (taskByUri)
            {
                if (! taskByUri.TryGetValue(uri, out task))
                {
                    task = this.service.QueryAsync(uri, token);
                    taskByUri.Add(uri, task);
                }
            }

            var result = await task;
            var intents = result.Intents;
            if (intents == null)
            {
                return null;
            }

            var intent = intents.SingleOrDefault(i => i.Intent.Equals(this.intent.IntentName, StringComparison.OrdinalIgnoreCase));
            if (intent == null)
            {
                return null;
            }

            // "builtin.intent.none" seems to have a null score

            var scope = new Scope(this.model, result, intent, resolver);
            scope.Item = resolver;
            scope.Scorable = this.inner;
            scope.State = await this.inner.PrepareAsync(scope, token);
            return scope;
        }

        protected override IntentRecommendation GetScore(IResolver resolver, Scope state)
        {
            return state.Intent;
        }

        protected override Task DoneAsync(IResolver item, Scope state, CancellationToken token)
        {
            try
            {
                Cache.Remove(item);
                return base.DoneAsync(item, state, token);
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

    public sealed class IntentComparer : IComparer<IntentRecommendation>
    {
        public static readonly IComparer<IntentRecommendation> Instance = new IntentComparer();
        private IntentComparer()
        {
        }

        int IComparer<IntentRecommendation>.Compare(IntentRecommendation one, IntentRecommendation two)
        {
            var scoreOne = one.Score.GetValueOrDefault();
            var scoreTwo = two.Score.GetValueOrDefault();
            return scoreOne.CompareTo(scoreTwo);
        }
    }
}
