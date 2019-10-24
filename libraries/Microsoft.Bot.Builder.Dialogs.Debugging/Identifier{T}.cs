// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// This class maintains an integer identifier for C# memory within the debug adapter
    /// that is referenced in the debug adapter protocol with Visual Studio Code.
    /// Examples include stack frames, values, and breakpoints.
    /// Ideally, identitiers fit within a 53 bit JavaScript Number.
    /// Ideally, identifiers can be recycled at some point.
    /// Some identifiers have a lifetime scoped to a thread (e.g. values or stack frames)
    /// For these combined identifiers, we use 7 bit encoding.
    /// </summary>
    /// <typeparam name="T">Datatype of the stored items.</typeparam>
    public sealed class Identifier<T> : IEnumerable<KeyValuePair<ulong, T>>
    {
        private readonly Dictionary<T, ulong> codeByItem = new Dictionary<T, ulong>(ReferenceEquality<T>.Instance);
        private readonly Dictionary<ulong, T> itemByCode = new Dictionary<ulong, T>();
        private readonly object gate = new object();
        private ulong last = 0;

        public IEnumerable<T> Items
        {
            get
            {
                lock (gate)
                {
                    return this.itemByCode.Values.ToArray();
                }
            }
        }

        public T this[ulong code]
        {
            get
            {
                lock (gate)
                {
                    return this.itemByCode[code];
                }
            }
        }

        public ulong this[T item]
        {
            get
            {
                lock (gate)
                {
                    return this.codeByItem[item];
                }
            }
        }

        public ulong Add(T item)
        {
            lock (gate)
            {
                if (!this.codeByItem.TryGetValue(item, out var code))
                {
                    // avoid false values
                    code = ++last;
                    this.codeByItem.Add(item, code);
                    this.itemByCode.Add(code, item);
                }

                return code;
            }
        }

        public void Remove(T item)
        {
            lock (gate)
            {
                var code = this.codeByItem[item];
                this.itemByCode.Remove(code);
                this.codeByItem.Remove(item);
            }
        }

        IEnumerator<KeyValuePair<ulong, T>> IEnumerable<KeyValuePair<ulong, T>>.GetEnumerator()
        {
            lock (gate)
            {
                return this.itemByCode.ToList().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<ulong, T>>)this).GetEnumerator();

        public bool TryGetValue(ulong code, out T item)
        {
            lock (gate)
            {
                return this.itemByCode.TryGetValue(code, out item);
            }
        }
    }
}
