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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Scorables
{
    /// <summary>
    /// Allow the scoring of items, with external comparison of scores, and enable the winner to take some action.
    /// </summary>
    /// <remarks>
    /// We avoided the traditional "bool TryScore(Item item, object state, out Score score)" pattern to allow for Score generic type parameter covariance.
    /// </remarks>
    public interface IScorable<in Item, out Score>
    {
        /// <summary>
        /// Perform some asynchronous work to analyze the item and produce some opaque state.
        /// </summary>
        Task<object> PrepareAsync(Item item, CancellationToken token);

        /// <summary>
        /// Returns whether this scorable wants to participate in scoring this item.
        /// </summary>
        bool HasScore(Item item, object state);

        /// <summary>
        /// Gets the score for this item.
        /// </summary>
        Score GetScore(Item item, object state);

        /// <summary>
        /// If this scorable wins, this method is called.
        /// </summary>
        Task PostAsync(Item item, object state, CancellationToken token);

        /// <summary>
        /// The scoring process has completed - dispose of any scoped resources.
        /// </summary>
        Task DoneAsync(Item item, object state, CancellationToken token);
    }
}