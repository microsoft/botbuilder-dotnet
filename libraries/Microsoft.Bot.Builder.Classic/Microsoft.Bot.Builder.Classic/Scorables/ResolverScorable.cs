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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Classic.Internals.Fibers;

namespace Microsoft.Bot.Builder.Classic.Scorables.Internals
{
    public abstract class ResolverScope<InnerScore> : Token<IResolver, InnerScore>, IResolver
    {
        protected readonly IResolver inner;

        public ResolverScope(IResolver inner)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }

        public virtual bool TryResolve(Type type, object tag, out object value)
        {
            return inner.TryResolve(type, tag, out value);
        }
    }

    [Serializable]
    public abstract class ResolverScorable<OuterState, OuterScore, InnerState, InnerScore> : ScorableAggregator<IResolver, OuterState, OuterScore, IResolver, InnerState, InnerScore>
        where OuterState : ResolverScope<InnerScore>
    {
        protected readonly IScorable<IResolver, InnerScore> inner;

        public ResolverScorable(IScorable<IResolver, InnerScore> inner)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }
    }
}
