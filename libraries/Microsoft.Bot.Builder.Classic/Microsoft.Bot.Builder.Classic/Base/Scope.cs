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

namespace Microsoft.Bot.Builder.Classic.Internals.Fibers
{
    /// <summary>
    /// Provide an abstraction to serialize access to an item for a using-block scope of code.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    public interface IScope<T>
    {
        /// <summary>
        /// Enter a scope of code keyed by item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A task whose completion produces an IDisposable for the scope.</returns>
        Task<IDisposable> WithScopeAsync(T item, CancellationToken token);
    }

    public sealed class LocalMutualExclusion<T> : IScope<T>
        where T : class
    {
        private sealed class KeyedGate
        {
            // this is the per-item semaphore
            public readonly SemaphoreSlim Gate = new SemaphoreSlim(initialCount: 1, maxCount: 1);

            // this property is protected by a monitor around gateByItem
            public int ReferenceCount = 0;
        }

        private readonly Dictionary<T, KeyedGate> gateByItem;

        public LocalMutualExclusion(IEqualityComparer<T> comparer)
        {
            this.gateByItem = new Dictionary<T, KeyedGate>(comparer);
        }

        public bool TryGetReferenceCount(T item, out int referenceCount)
        {
            lock (this.gateByItem)
            {
                KeyedGate gate;
                if (this.gateByItem.TryGetValue(item, out gate))
                {
                    referenceCount = gate.ReferenceCount;
                    return true;
                }
            }

            referenceCount = 0;
            return false;
        }

        async Task<IDisposable> IScope<T>.WithScopeAsync(T item, CancellationToken token)
        {
            // manage reference count under global mutex
            KeyedGate gate;
            lock (this.gateByItem)
            {
                gate = this.gateByItem.GetOrAdd(item, _ => new KeyedGate());
                ++gate.ReferenceCount;
            }

            // wait to enter this item's semaphore outside of global mutex
            await gate.Gate.WaitAsync(token);

            return new Releaser(this, item);
        }

        private sealed class Releaser : IDisposable
        {
            private readonly LocalMutualExclusion<T> owner;
            private readonly T item;
            public Releaser(LocalMutualExclusion<T> owner, T item)
            {
                SetField.NotNull(out this.owner, nameof(owner), owner);
                SetField.NotNull(out this.item, nameof(item), item);
            }
            public void Dispose()
            {
                KeyedGate gate;
                lock (this.owner.gateByItem)
                {
                    gate = this.owner.gateByItem[this.item];
                }

                // exit this item's semaphore outside of global mutex, and let other threads run here
                gate.Gate.Release();

                // obtain the global mutex to update the reference count
                lock (this.owner.gateByItem)
                {
                    --gate.ReferenceCount;

                    // and possibly clean up the semaphore
                    // if there are no other threads referencing this item's semaphore
                    if (gate.ReferenceCount == 0)
                    {
                        if (!this.owner.gateByItem.Remove(this.item))
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }
            }
        }
    }
}
