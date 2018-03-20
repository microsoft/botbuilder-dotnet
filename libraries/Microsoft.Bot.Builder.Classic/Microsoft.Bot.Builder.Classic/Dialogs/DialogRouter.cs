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
using System.Linq;

using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Scorables.Internals;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Classic.Scorables;

namespace Microsoft.Bot.Builder.Classic.Dialogs.Internals
{
    /// <summary>
    /// Scorable for Dialog module routing.
    /// </summary>
    public sealed class DialogRouter : DelegatingScorable<IActivity, double>
    {
        public static IEnumerable<IScorable<IActivity, double>> EnumerateRelevant(
            IDialogStack stack,
            IEnumerable<IScorable<IActivity, double>> fromActivity,
            IEnumerable<IScorable<IResolver, double>> fromResolver,
            Func<IActivity, IResolver> makeResolver)
        {
            // first, let's go through stack frames
            var targets = stack.Frames.Select(f => f.Target);
            foreach (var target in targets)
            {
                var activityScorable = target as IScorable<IActivity, double>;
                if (activityScorable != null)
                {
                    yield return activityScorable;
                }

                var resolverScorable = target as IScorable<IResolver, double>;
                if (resolverScorable != null)
                {
                    yield return resolverScorable.SelectItem(makeResolver);
                }
            }

            // then global scorables "on the side"
            foreach (var activityScorable in fromActivity)
            {
                yield return activityScorable;
            }

            foreach (var resolverScorable in fromResolver)
            {
                yield return resolverScorable.SelectItem(makeResolver);
            }
        }

        public static IScorable<IActivity, double> MakeDelegate(
            IDialogStack stack,
            IEnumerable<IScorable<IActivity, double>> fromActivity,
            IEnumerable<IScorable<IResolver, double>> fromResolver,
            Func<IActivity, IResolver> makeResolver,
            ITraits<double> traits,
            IComparer<double> comparer)
        {
            // since the stack of scorables changes over time, this should be lazy
            var relevant = EnumerateRelevant(stack, fromActivity, fromResolver, makeResolver);
            var significant = relevant.Select(s => s.WhereScore((_, score) => comparer.Compare(score, traits.Minimum) >= 0));
            var scorable = new TraitsScorable<IActivity, double>(traits, comparer, significant);
            return scorable;
        }

        public DialogRouter(
            IDialogStack stack,
            IEnumerable<IScorable<IActivity, double>> fromActivity,
            IEnumerable<IScorable<IResolver, double>> fromResolver,
            Func<IActivity, IResolver> makeResolver,
            ITraits<double> traits,
            IComparer<double> comparer)
            : base(MakeDelegate(stack, fromActivity, fromResolver, makeResolver, traits, comparer))
        {
        }
    }
}
